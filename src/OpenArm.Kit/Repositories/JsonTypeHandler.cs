using System.Data;
using System.Text.Json;
using static Dapper.SqlMapper;

namespace OpenArm.Repositories
{
    internal class JsonTypeHandler<T> : TypeHandler<T>
    {
        private readonly JsonSerializerOptions options;

        public JsonTypeHandler(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public override T Parse(object value)
        {
            var json = (string)value;
            return JsonSerializer.Deserialize<T>(json, options)!;
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.DbType = DbType.String;

            // This NEEDS to be object. Don't change it.
            //
            // SqlMapper uses the declared type of the parameter object for type handler binding (not the runtime type).
            //
            // So for a case like WidgetProperties, the declared type is actually ResourceProperties when we're doing
            // a write operation. Since we're using reflection to make the serialization dynamic on the write side,
            // we need to make sure the serializer sees the runtime type - and we can do that by passing on object here.
            parameter.Value = JsonSerializer.Serialize<object>(value!, options);
        }
    }
}