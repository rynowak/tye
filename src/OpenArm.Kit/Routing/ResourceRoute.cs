using System.Text;
using Azure.Deployments.Core.Extensions;

namespace OpenArm
{
    public static class ResourceRoute
    {
        // Builds a route like /subscriptions/{}/resourceGroups/someType/{}/someothertype
        // for use as the base for a controller.
        public static string Build(string @namespace, string resourceType, params string[] additionalTypes)
        {
            var template = new StringBuilder("/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers");

            template.Append($"/{@namespace}/{resourceType}");
            if (additionalTypes.Length > 0)
            {
                template.Append("/{{type}}");
            }
            
            for (var i = 0; i < additionalTypes.Length; i++)
            {
                template.Append($"/{additionalTypes[i]}");

                if (i != additionalTypes.Length - 1)
                {
                    template.Append($"/{{type{i+1}}}");
                }
            }

            return template.ToString();
        }
    }
}