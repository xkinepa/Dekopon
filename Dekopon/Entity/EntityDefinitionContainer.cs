using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dekopon.Attributes;
using Dekopon.Miscs;

namespace Dekopon.Entity
{
    public class EntityDefinitionContainer
    {
        private static readonly Lazy<EntityDefinitionContainer> EntityDefinitionContainerHolder =
            new Lazy<EntityDefinitionContainer>(() => new EntityDefinitionContainer());

        public static EntityDefinitionContainer Instance => EntityDefinitionContainerHolder.Value;

        private readonly ConcurrentDictionary<Type, EntityDefinition> _container =
            new ConcurrentDictionary<Type, EntityDefinition>();

        private EntityDefinitionContainer()
        {
        }

        public EntityDefinition Get(Type t)
        {
            return _container.GetOrAdd(t, type =>
            {
                var table = type.GetCustomAttribute<TableAttribute>(false);
                Assertion.NotNull(table, $"{type.Name} has no {nameof(TableAttribute)}");

                var where = type.GetCustomAttribute<WhereAttribute>(true);
                var delete = type.GetCustomAttribute<DeleteAttribute>(true);
                var properties = type.GetRuntimeProperties().Select(p => new
                {
                    Property = p,
                    Column = p.GetCustomAttribute<ColumnAttribute>(true),
                    Key = p.GetCustomAttribute<KeyAttribute>(true),
                    Generated = p.GetCustomAttribute<GeneratedAttribute>(true),
                    Convert = p.GetCustomAttribute<ConvertAttribute>(true),
                }).Select(it => new ColumnDefinition
                {
                    Property = it.Property,
                    Getter = it.Property.CanRead ? GetGetter(type, it.Property) : null,
                    Setter = it.Property.CanWrite ? GetSetter(type, it.Property) : null,
                    Name = it.Column?.Name ?? it.Property.Name,
                    DbType = it.Column?.DbType,
                    Convert = it.Convert?.Pattern,
                    Generated = it.Generated != null,
                    Key = it.Key != null,
                    Id = it.Key?.IsIdentity ?? false,
                }).ToImmutableList();
                var definition = new EntityDefinition
                {
                    Type = type,
                    Table = table?.Name ?? type.Name,
                    Where = where?.Clause,
                    SetForDelete = delete?.Set,
                    Columns = properties,
                };

                definition.IdColumn = definition.Columns.SingleOrDefault(it => it.Id);
                Assertion.IsTrue(definition.IdColumn == null || definition.IdColumn.Property.PropertyType == typeof(long), $"idColumn's type should be {nameof(Int64)}");

                return definition;
            });
        }

        private Func<object, object> GetGetter(Type type, PropertyInfo property)
        {
            var getParamObj = Expression.Parameter(typeof(object));
            var getterExpr = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Property(Expression.Convert(getParamObj, type), property.Name),
                    typeof(object)
                ),
                getParamObj
            );
            return getterExpr.Compile();
        }

        private Action<object, object> GetSetter(Type type, PropertyInfo property)
        {
            var setParamObj = Expression.Parameter(typeof(object));
            var setParamVal = Expression.Parameter(typeof(object));
            var setterExpr = Expression.Lambda<Action<object, object>>(
                Expression.Assign(
                    Expression.Property(Expression.Convert(setParamObj, type), property.Name),
                    Expression.Convert(setParamVal, property.PropertyType)
                ),
                setParamObj, setParamVal
            );
            return setterExpr.Compile();
        }

        private SqlDbType? ParseDbType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            Assertion.IsTrue(Enum.TryParse<SqlDbType>(type, true, out var dbType), $"could not parse dbType value: {type}");
            return dbType;
        }
    }
}