using System;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OpenArm
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ResourceRouteAttribute(string @namespace, string resourceType, params string[] additionalTypes)
        {
            Template = ResourceRoute.Build(@namespace, resourceType, additionalTypes);
        }

        public string Template { get; }

        public int? Order => null;

        public string? Name => null;
    }
}