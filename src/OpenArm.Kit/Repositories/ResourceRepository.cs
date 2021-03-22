using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using Dapper;
using OpenArm.Resources;

namespace OpenArm.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly Func<IDbConnection> connect;

        public ResourceRepository(Func<IDbConnection> connection)
        {
            this.connect = connection;
        }

        private static string FormatNormalizedId(ResourceGroupLevelResourceId id)
            => id.ToString().ToLowerInvariant();
        
        public async Task InitializeAsync()
        {
            using var connection = this.connect(); 
            var table = await connection.QueryAsync<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Resource';");
            var tableName = table.FirstOrDefault();

            if (!string.IsNullOrEmpty(tableName))
            {
                return;
            }

            await connection.ExecuteAsync(@"
CREATE TABLE Resource (
    NormalizedId VARCHAR(256) primary key,
    NormalizedSubscriptionId VARCHAR(36) NOT NULL,
    NormalizedResourceGroup VARCHAR(90) NOT NULL,
    Type VARCHAR(90) NOT NULL,
    Id VARCHAR(256) NOT NULL,
    SubscriptionId VARCHAR(36) NOT NULL,
    ResourceGroup VARCHAR(60) NOT NULL,
    Name VARCHAR(36) NOT NULL,
    Properties TEXT NOT NULL
);");
        }

        public async Task<IEnumerable<TResource>> List<TResource>(string subscriptionId, string resourceGroup) 
            where TResource : Resource
        {
            using var connection = this.connect(); 
            var type = typeof(TResource).GetCustomAttribute<ResourceTypeAttribute>()?.Type;
            return await connection.QueryAsync<TResource>(@"
SELECT NormalizedId, Type, Id, SubscriptionId, ResourceGroup, Name, Properties
FROM Resource
WHERE NormalizedSubscriptionId = @NormalizedSubscriptionId AND NormalizedResourceGroup = @NormalizedResourceGroup AND Type = @Type;", 
                new {
                    NormalizedSubscriptionId = subscriptionId.ToLowerInvariant(),
                    NormalizedResourceGroup = resourceGroup.ToLowerInvariant(),
                    Type = type,
                });
        }

        public async Task<TResource?> Get<TResource>(ResourceGroupLevelResourceId id) where TResource : Resource
        {
            using var connection = this.connect(); 
            return await connection.QuerySingleOrDefaultAsync<TResource>(@"
SELECT NormalizedId, Type, Id, SubscriptionId, ResourceGroup, Name, Properties
FROM Resource
WHERE NormalizedId = @NormalizedId;", 
                new {
                    NormalizedId = FormatNormalizedId(id),
                });
        }

        public async Task Upsert<TResource>(TResource resource) where TResource : Resource
        {
            using var connection = this.connect(); 
            await connection.ExecuteAsync(@"
INSERT INTO Resource
( NormalizedId, NormalizedSubscriptionId, NormalizedResourceGroup, Type, Id, SubscriptionId, ResourceGroup, Name, Properties) VALUES
( @NormalizedId, @NormalizedSubscriptionId, @NormalizedResourceGroup, @Type, @Id, @SubscriptionId, @ResourceGroup, @Name, @Properties )
ON CONFLICT(NormalizedID) 
DO UPDATE SET 
    NormalizedSubscriptionId=excluded.NormalizedSubscriptionId, 
    NormalizedResourceGroup=excluded.NormalizedResourceGroup,
    Type=excluded.Type,
    ID=excluded.ID, 
    SubscriptionId=excluded.SubscriptionId, 
    ResourceGroup=excluded.ResourceGroup,
    Name=excluded.Name, 
    Properties=excluded.Properties",
                new {
                    NormalizedId = resource.NormalizedId,
                    NormalizedSubscriptionId = resource.SubscriptionId.ToLowerInvariant(),
                    NormalizedResourceGroup = resource.ResourceGroup.ToLowerInvariant(),
                    Type = resource.Type,
                    Id = resource.Id,
                    SubscriptionId = resource.SubscriptionId,
                    ResourceGroup = resource.ResourceGroup,
                    Name = resource.Name,
                    Properties = (ResourceProperties?)resource.GetType().GetProperty("Properties")?.GetValue(resource) ?? new ResourceProperties(),
                });
        }

        public async Task Delete(ResourceGroupLevelResourceId id)
        {
            using var connection = this.connect(); 
            await connection.ExecuteAsync(@"
DELETE FROM Resource
WHERE NormalizedId = @NormalizedId;",
                new {
                    NormalizedId = FormatNormalizedId(id),
                });
        }
    }
}