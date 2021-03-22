using System;
using System.ComponentModel.DataAnnotations;

namespace OpenArm
{
    public class ProviderRegistration
    {
        [Required]
        public string DisplayName { get; set; } = default!;

        [Required]
        public string Uri { get; set; } = default!;

        public string[] ResourceTypes { get; set; } = Array.Empty<string>();

        public string[] ApiVersions { get; set; } = new string[]{ "2020-12-31", };
    }
}