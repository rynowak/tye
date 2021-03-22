using System.Data;
using Azure.Deployments.Core.Json;
using Newtonsoft.Json;
using static Dapper.SqlMapper;

namespace OpenArm.Repositories
{
    internal class NewtonsoftJsonTypeHandler<T> : TypeHandler<T>
    {
        public override T Parse(object value)
        {
            var json = (string)value;
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings.SerializerObjectTypeSettings);
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = JsonConvert.SerializeObject(value, SerializerSettings.SerializerObjectTypeSettings);
        }
    }
}