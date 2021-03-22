using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenArm.Resources
{
    public class Resource
    {
        [Required]
        public string Id { get; set; } = default!;

        [Required]
        [JsonIgnore]
        public string NormalizedId { get; set; } = default!;

        [Required]
        [JsonIgnore]
        public string SubscriptionId { get; set; } = default!;

        [Required]
        [JsonIgnore]
        public string ResourceGroup { get; set; } = default!;

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Type { get; set; } = default!;
    }
}