using System;

namespace OpenArm.Resources
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourceTypeAttribute : Attribute
    {
        public ResourceTypeAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
}