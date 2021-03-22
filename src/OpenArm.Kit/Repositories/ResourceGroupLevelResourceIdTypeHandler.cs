using System.Data;
using Azure.Deployments.Core.Definitions.Identifiers;
using Dapper;

namespace OpenArm.Repositories
{
    public class ResourceGroupLevelResourceIdTypeHandler : SqlMapper.TypeHandler<ResourceGroupLevelResourceId>
    {
        public override void SetValue(IDbDataParameter parameter, ResourceGroupLevelResourceId value)
        {
            parameter.Value = value.ToString();
            parameter.DbType = DbType.String;
        }

        public override ResourceGroupLevelResourceId Parse(object value)
        {
            return ResourceGroupLevelResourceId.Parse(value.ToString());
        }
    }
}
