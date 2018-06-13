using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Xunit;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncDiagramGenerator
    {
        class Configurator : ITransitionConfigurator
        {
            public List<object> AllDistinctStatesInOrder { get; } = new List<object>();

            public Dictionary<IStateResult, (object State, Type ParameterType)> Transitions { get; }
                = new Dictionary<IStateResult, (object, Type)>();

            public void ConfigureTransition(IStateResult result, ISyncState state)
            {
                addToListIfNew(state);
                Transitions.Add(result, (state, null));
            }

            public void ConfigureTransition<T>(StateResult<T> result, ISyncState<T> state)
            {
                addToListIfNew(state);
                Transitions.Add(result, (state, typeof(T)));
            }

            private void addToListIfNew(object state)
            {
                if (AllDistinctStatesInOrder.Contains(state))
                    return;

                AllDistinctStatesInOrder.Add(state);
            }
        }

        class Node
        {
            public enum NodeType
            {
                Regular = 0,
                EntryPoint = 1,
                DeadEnd = 2,
                InvalidTransitionState = 3,
            }

            public string Id { get; set; }
            public string Label { get; set; }
            public NodeType Type { get; set; }
        }

        class Edge
        {
            public string Label { get; set; }
            public Node From { get; set; }
            public Node To { get; set; }
        }

        [Fact, LogIfTooSlow]
        public void GenerateDOTFile()
        {
            var entryPoints = new StateMachineEntryPoints();
            var configurator = new Configurator();
            configureTransitions(configurator, entryPoints);

            var allStateResults = getAllStateResultsByState(configurator.AllDistinctStatesInOrder);
            var stateNodes = makeNodesForStates(configurator.AllDistinctStatesInOrder);

            var edges = getEdgesBetweenStates(allStateResults, configurator, stateNodes);
            var nodes = stateNodes.Values.ToList();

            addEntryPoints(edges, nodes, entryPoints, configurator, stateNodes);
            addDeadEnds(edges, nodes, allStateResults, configurator, stateNodes);

            idNodes(nodes);

            var fileContent = writeDotFile(nodes, edges);

            File.WriteAllText("sync-graph.gv", fileContent);
        }

        private string writeDotFile(List<Node> nodes, List<Edge> edges)
        {
            var builder = new StringBuilder();

            builder.AppendLine("digraph SyncGraph {");

            foreach (var node in nodes)
            {
                var nodeAttributes = getAttributes(node);
                var attributeString = string.Join(",", nodeAttributes.Select(a => $"{a.Key}=\"{a.Value}\""));
                builder.AppendLine($"{node.Id} [{attributeString}];");
            }

            foreach (var edge in edges)
            {
                builder.AppendLine($"{edge.From.Id} -> {edge.To.Id} [label=\"{edge.Label}\"];");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private List<(string Key, string Value)> getAttributes(Node node)
        {
            var attributes = new List<(string, string)>
            {
                ("label", node.Label)
            };

            switch (node.Type)
            {
                case Node.NodeType.EntryPoint:
                    attributes.Add(("color", "green"));
                    break;
                case Node.NodeType.DeadEnd:
                    attributes.Add(("color", "orange"));
                    break;
                case Node.NodeType.InvalidTransitionState:
                    attributes.Add(("color", "red"));
                    break;
            }

            return attributes;
        }

        private void idNodes(List<Node> nodes)
        {
            string previousId = null;
            var i = 0;
            foreach (var node in nodes.OrderBy(n => n.Label))
            {
                var id = node.Label;
                if (id != previousId)
                {
                    node.Id = $"\"{id}\"";
                    i = 0;
                }
                else
                {
                    i++;
                    node.Id = $"\"{id + i}\"";
                }
                previousId = id;
            }
        }

        private void addDeadEnds(List<Edge> edges, List<Node> nodes,
            List<(object State, List<(IStateResult Result, string Name)> StateResults)> allStateResults, Configurator configurator,
            Dictionary<object, Node> stateNodes)
        {
            foreach (var (state, result) in allStateResults
                .SelectMany(results => results.StateResults
                    .Where(r => !configurator.Transitions.ContainsKey(r.Result))
                    .Select(r => (results.State, r))))
            {
                var node = new Node
                {
                    Label = "Dead End",
                    Type = Node.NodeType.DeadEnd
                };
                nodes.Add(node);

                var edge = new Edge
                {
                    From = stateNodes[state],
                    To = node,
                    Label = result.Name
                };
                edges.Add(edge);
            }
        }

        private void addEntryPoints(List<Edge> edges, List<Node> nodes, StateMachineEntryPoints entryPoints,
            Configurator configurator, Dictionary<object, Node> stateNodes)
        {
            foreach (var (property, stateResult) in entryPoints.GetType()
                .GetProperties()
                .Where(isStateResultProperty)
                .Select(p => (p, (IStateResult)p.GetValue(entryPoints))))
            {
                var node = new Node
                {
                    Label = property.Name,
                    Type = Node.NodeType.EntryPoint
                };
                nodes.Add(node);

                if (configurator.Transitions.TryGetValue(stateResult, out var state))
                {
                    var edge = new Edge
                    {
                        From = node,
                        To = stateNodes[state.State],
                        Label = ""
                    };
                    edges.Add(edge);
                }
            }


        }

        private List<Edge> getEdgesBetweenStates(
            List<(object State, List<(IStateResult Result, string Name)> StateResults)> allStateResults,
            Configurator configurator, Dictionary<object, Node> stateNodes)
        {
            return allStateResults
                .SelectMany(results =>
                    results.StateResults
                        .Where(sr => configurator.Transitions.ContainsKey(sr.Result))
                        .Select(sr => edge(results.State, configurator.Transitions[sr.Result], stateNodes, sr.Name))
                )
                .ToList();
        }

        private Edge edge(object fromState, (object State, Type ParameterType) transition,
            Dictionary<object, Node> stateNodes, string name)
        {
            return new Edge
            {
                From = stateNodes[fromState],
                To = stateNodes[transition.State],
                Label = transition.ParameterType == null
                    ? name
                    : $"{name}<{fullGenericTypeName(transition.ParameterType)}>"
            };
        }

        private Dictionary<object, Node> makeNodesForStates(List<object> allStates)
        {
            return allStates.ToDictionary(s => s,
                s => new Node
                {
                    Label = fullGenericTypeName(s.GetType()),
                    Type = s is InvalidTransitionState ? Node.NodeType.InvalidTransitionState : Node.NodeType.Regular
                });
        }

        private string fullGenericTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericArgumentNames = type.GetGenericArguments().Select(fullGenericTypeName);

            if (typeof(ITuple).IsAssignableFrom(type))
            {
                return $"({string.Join(", ", genericArgumentNames)})";
            }

            var cleanedName = type.Name;
            var backTickIndex = cleanedName.IndexOf('`');
            if (backTickIndex >= 0)
                cleanedName = cleanedName.Substring(0, backTickIndex);

            return $"{cleanedName}<{string.Join(", ", genericArgumentNames)}>";
        }

        private static List<(object State, List<(IStateResult Result, string Name)> StateResults)> getAllStateResultsByState(
            List<object> allStates)
        {
            return allStates
                .Select(state => (state, state.GetType()
                    .GetProperties()
                    .Where(isStateResultProperty)
                    .Select(p => ((IStateResult)p.GetValue(state), p.Name))
                    .ToList())
                ).ToList();
        }

        private static bool isStateResultProperty(PropertyInfo p)
        {
            return typeof(IStateResult).IsAssignableFrom(p.PropertyType);
        }

        private static void configureTransitions(Configurator configurator, StateMachineEntryPoints entryPoints)
        {
            TogglSyncManager.ConfigureTransitions(
                configurator,
                Substitute.For<ITogglDatabase>(),
                Substitute.For<ITogglApi>(),
                Substitute.For<ITogglDataSource>(),
                Substitute.For<IRetryDelayService>(),
                Substitute.For<IScheduler>(),
                Substitute.For<ITimeService>(),
                Substitute.For<IAnalyticsService>(),
                entryPoints,
                Substitute.For<IObservable<Unit>>()
            );
        }
    }
}
