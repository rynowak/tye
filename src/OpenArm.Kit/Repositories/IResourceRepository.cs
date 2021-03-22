using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using OpenArm.Resources;

namespace OpenArm.Repositories
{
    public interface IResourceRepository : IRepository
    {
        // TODO handle nested resources!
        Task<IEnumerable<TResource>> List<TResource>(string subscriptionId, string resourceGroup)
            where TResource : Resource;

        Task<TResource?> Get<TResource>(ResourceGroupLevelResourceId id) where TResource : Resource;

        Task Upsert<TResource>(TResource resource) where TResource : Resource;

        Task Delete(ResourceGroupLevelResourceId id);
    }
}