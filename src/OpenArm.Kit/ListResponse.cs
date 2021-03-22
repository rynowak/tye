using System.Collections.Immutable;

namespace OpenArm
{
    // https://github.com/Azure/azure-resource-manager-rpc/blob/master/v1.0/resource-api-reference.md#get-resource contains list paging contract
    public class ListResponse<T>
    {
        public ImmutableArray<T> Value { get; set; }

        public string? NextLink { get; set; }
    }
}
