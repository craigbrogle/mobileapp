using System;

namespace Toggl.Foundation.Analytics
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AnalyticsEventAttribute : Attribute
    {
        public string[] ParameterNames { get; }

        public AnalyticsEventAttribute(params string[] parameterNames)
        {
            ParameterNames = parameterNames;
        }
    }
}
