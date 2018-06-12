using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Serialization.Attributes;

namespace Toggl.Ultrawave.Serialization
{
    internal sealed class RequiresFeatureAttributeFilter : IPropertiesFilter
    {
        private readonly IWorkspaceFeatureCollection featuresCollection;

        public RequiresFeatureAttributeFilter(IWorkspaceFeatureCollection featuresCollection)
        {
            this.featuresCollection = featuresCollection;
        }

        public IList<JsonProperty> Filter(IList<JsonProperty> properties)
        {
            if (featuresCollection == null)
                return properties;

            foreach (JsonProperty property in properties)
            {
                var attributes = property.AttributeProvider.GetAttributes(typeof(RequiresFeatureAttribute), false);
                if (attributes.OfType<RequiresFeatureAttribute>().Any(isNotEnabled))
                    property.ShouldSerialize = _ => false;
            }

            return properties;
        }

        private bool isNotEnabled(RequiresFeatureAttribute attribute)
            => !featuresCollection.IsEnabled(attribute.RequiredFeature);
    }
}