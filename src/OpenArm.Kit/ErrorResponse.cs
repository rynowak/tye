
namespace OpenArm
{
    // https://github.com/Azure/azure-resource-manager-rpc/blob/master/v1.0/common-api-details.md#error-response-content
    public class ErrorResponse
    {
        public ExtendedError Error { get; set; } = default!;
    }
}
