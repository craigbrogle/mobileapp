using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Toggl.Foundation.Analytics
{
    internal sealed class TrackableEventTransformation
    {
        public string EventName { get; }
        public bool IsValid { get; } = true;

        public EventTransformationHandler Transform { get; }

        private static readonly Type stringType = typeof(string);
        private static readonly Type dictionaryType = typeof(Dictionary<string, string>);
        private static readonly Type trackableEventType = typeof(ITrackableEvent);

        public TrackableEventTransformation(ITrackableEvent trackableEvent)
        {
            var eventType = trackableEvent.GetType();
            EventName = eventType.NameOrDisplayName();

            var addMethod = dictionaryType.GetMethod("Add");
            if (addMethod == null)
            {
                IsValid = false;
                return;
            }

            try
            {
                var dictParameter = Expression.Parameter(dictionaryType, "parameters");
                var trackableEventParameter = Expression.Parameter(trackableEventType, "trackableEvent");

                var upCastExpression = Expression.Convert(trackableEventParameter, eventType);

                var props = new List<Expression>();

                foreach (var property in eventType.GetProperties(Public | Instance))
                {
                    var propertyName = property.NameOrDisplayName();
                    var propertyNameExpression = Expression.Constant(propertyName, stringType);

                    var propertyAccess = Expression.Property(upCastExpression, property);
                    var toStringCallExpression = Expression.Call(propertyAccess, "ToString", Type.EmptyTypes);

                    var call = Expression.Call(dictParameter, addMethod, propertyNameExpression, toStringCallExpression);
                    props.Add(call);
                }

                var returnTarget = Expression.Label(dictionaryType);
                var returnExpression = Expression.Return(returnTarget, dictParameter, dictionaryType);
                var returnLabel = Expression.Label(returnTarget, Expression.Constant(new Dictionary<string, string>()));
                props.Add(returnExpression);
                props.Add(returnLabel);

                var block = Expression.Block(props);

                var func = Expression.Lambda<EventTransformationHandler>(block, trackableEventParameter, dictParameter).Compile();

                Transform = func;
            }
            catch
            {
                IsValid = false;
            }
        }

    }
}
