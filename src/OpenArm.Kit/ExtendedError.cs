using System.Collections.Immutable;

namespace OpenArm
{
    // https://github.com/Azure/azure-resource-manager-rpc/blob/master/v1.0/common-api-details.md#error-response-content
    public class ExtendedError
    {
        public string Code { get; set; } = default!;

        public string Message { get; set; } = default!;

        public string? Target { get; set; } = default!;

        public ImmutableArray<ExtendedError>? Details { get; set; }
    }
}
