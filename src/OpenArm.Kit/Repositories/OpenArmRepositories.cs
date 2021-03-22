using System;
using System.Linq;
using System.Text.Json;
using Dapper;

namespace OpenArm.Repositories
{
    public static class OpenArmRepositories
    {
        public static void RegisterNewtonsoftJsonType(Type type)
        {
            var mapperType = typeof(NewtonsoftJsonTypeHandler<>).MakeGenericType(type);
            var mapper = (SqlMapper.ITypeHandler)mapperType.GetConstructors().Single().Invoke(Array.Empty<object>());
            SqlMapper.AddTypeHandler(type, mapper);
        }

        public static void RegisterJsonType(Type type, JsonSerializerOptions jsonSerializerOptions)
        {
            var mapperType = typeof(JsonTypeHandler<>).MakeGenericType(type);
            var mapper = (SqlMapper.ITypeHandler)mapperType.GetConstructors().Single().Invoke(new object[]{ jsonSerializerOptions, });
            SqlMapper.AddTypeHandler(type, mapper);
        }
    }
}