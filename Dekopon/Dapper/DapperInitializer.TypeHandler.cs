using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Dekopon.Dapper
{
    public static partial class DapperInitializer
    {
        public static void Register<TTypeHandler, TType>(IEnumerable<TTypeHandler> typeHandlerList)
            where TTypeHandler : SqlMapper.TypeHandler<TType>
        {
            foreach (var typeHandler in typeHandlerList)
            {
                SqlMapper.AddTypeHandler(typeHandler);
            }
        }

        public static void Register(IEnumerable<SqlMapper.ITypeHandler> typeHandlerList)
        {
            foreach (var typeHandler in typeHandlerList)
            {
                Register(typeHandler);
            }
        }

        public static void Register(SqlMapper.ITypeHandler typeHandler)
        {
            for (var type = typeHandler.GetType(); typeof(object) != type; type = type.BaseType)
            {
                if (type == null)
                {
                    break;
                }

                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SqlMapper.TypeHandler<>))
                {
                    continue;
                }

                SqlMapper.AddTypeHandler(type.GenericTypeArguments.Single(), typeHandler);
                return;
            }

            // logger;
        }

        public static void Register<T>(SqlMapper.TypeHandler<T> typeHandler)
        {
            SqlMapper.AddTypeHandler(typeHandler);
        }
    }
}