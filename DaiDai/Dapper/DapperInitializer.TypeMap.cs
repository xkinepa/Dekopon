using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DaiDai.Attributes;
using DaiDai.Entity;
using DaiDai.Miscs;
using Dapper;

namespace DaiDai.Dapper
{
    public static partial class DapperInitializer
    {
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> ColumnNameCache = new ConcurrentDictionary<(Type, string), PropertyInfo>();

        private static readonly Func<Type, SqlMapper.ITypeMap> DefaultTypeMapProvider = SqlMapper.TypeMapProvider;
        private static readonly IList<(Func<Type, bool>, Func<Type, SqlMapper.ITypeMap>)> TypeMapProviderList = Enumerables.List<(Func<Type, bool>, Func<Type, SqlMapper.ITypeMap>)>();

        public static void RegisterTypeMap(Func<Type, bool> typeSelector, Func<Type, SqlMapper.ITypeMap> typeMapProvider)
        {
            TypeMapProviderList.Add((typeSelector, typeMapProvider));

            if (SqlMapper.TypeMapProvider == DefaultTypeMapProvider)
            {
                SqlMapper.TypeMapProvider = type =>
                {
                    foreach (var (selector, mapProvider) in TypeMapProviderList)
                    {
                        if (selector.Invoke(type))
                        {
                            return mapProvider.Invoke(type);
                        }
                    }

                    return DefaultTypeMapProvider.Invoke(type);
                };
            }
        }

        public static void RegisterAnnotatedTypeMap()
        {
            SqlMapper.ITypeMap CustomPropertyTypeMapProvider(Type t) => new CustomPropertyTypeMap(t,
                (type, columnName) => ColumnNameCache.GetOrAdd((type, columnName), RetrieveProperty));

            PropertyInfo RetrieveProperty((Type, string) typeColumnName)
            {
                var (type, columnName) = typeColumnName;

                var entityDefinition = EntityDefinitionContainer.Instance.Get(type);
                return entityDefinition.Columns.SingleOrDefault(it => string.Equals(it.Name, columnName, StringComparison.InvariantCultureIgnoreCase))?.Property;
            }

            RegisterTypeMap(type => type.GetCustomAttribute<TableAttribute>() != null, CustomPropertyTypeMapProvider);
        }
    }
}