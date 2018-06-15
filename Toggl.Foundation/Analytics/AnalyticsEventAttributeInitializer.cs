using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Toggl.Multivac;

namespace Toggl.Foundation.Analytics
{
    [Preserve(AllMembers = true)]
    public abstract class AnalyticsEventAttributeInitializer
    {
        private readonly Dictionary<int, Type> genericTypes = new Dictionary<int, Type>
        {
            [1] = typeof(AnalyticsEvent<>),
            [2] = typeof(AnalyticsEvent<,>),
            [3] = typeof(AnalyticsEvent<,,>),
            [4] = typeof(AnalyticsEvent<,,,>)
        };

        protected void InitializeAttributedProperties(IAnalyticsService analyticsService)
        {
            var eventPropertiesInfo = GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(AnalyticsEventAttribute)));
            foreach (var eventPropertyInfo in eventPropertiesInfo)
            {
                var attribute = eventPropertyInfo.GetCustomAttribute<AnalyticsEventAttribute>();
                var type = eventPropertyInfo.PropertyType;
                var name = eventPropertyInfo.Name;

                if (!type.IsGenericType)
                {
                    if (attribute.ParameterNames.Length != 0)
                    {
                        throw new InvalidOperationException(
                            $"The type {type.FullName} cannot accept any parameter names but insted received {attribute.ParameterNames.Length}.");
                    }

                    var property = new AnalyticsEvent(analyticsService, name);
                    eventPropertyInfo.SetValue(this, property);
                }
                else
                {
                    if (attribute.ParameterNames.Length != type.GenericTypeArguments.Length)
                    {
                        throw new InvalidOperationException(
                            $"The type {type.FullName} needs {type.GenericTypeArguments.Length} parameter names but insted received {attribute.ParameterNames.Length}.");
                    }

                    var genericType = getGenericType(type.GenericTypeArguments);
                    var constructorInfo = genericType.GetConstructor(new[] { typeof(IAnalyticsService), typeof(string), typeof(string[]) });

                    if (constructorInfo == null)
                    {
                        throw new InvalidOperationException(
                            $"It is not possible to instantiate {genericType} - the constructor info was probably stripped by the linker.");
                    }

                    var property = constructorInfo.Invoke(new object[] { analyticsService, name, attribute.ParameterNames });
                    eventPropertyInfo.SetValue(this, property);
                }
            }
        }

        private Type getGenericType(Type[] genericTypeArguments)
        {
            if (genericTypes.TryGetValue(genericTypeArguments.Length, out var type))
            {
                return type.MakeGenericType(genericTypeArguments);
            }

            throw new ArgumentOutOfRangeException($"There is not an analytics event type for {genericTypeArguments.Length} parameters.");
        }
    }
}
