﻿using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DaiDai.Attributes;

namespace DaiDai.Entity
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
                var where = type.GetCustomAttribute<WhereAttribute>(true);
                var delete = type.GetCustomAttribute<DeleteAttribute>(true);
                var properties = type.GetRuntimeProperties().Select(p => new
                {
                    Property = p,
                    Column = p.GetCustomAttribute<ColumnAttribute>(true),
                    Key = p.GetCustomAttribute<KeyAttribute>(true),
                    Generated = p.GetCustomAttribute<GeneratedAttribute>(true),
                });
                var definition = new EntityDefinition
                {
                    Type = type,
                    Table = table?.Name ?? type.Name,
                    Where = where?.Clause,
                    SetForDelete = delete?.Set,
                    Columns = properties
                        .Select(it => new ColumnDefinition
                        {
                            Property = it.Property,
                            Getter = it.Property.CanRead ? GetGetter(type, it.Property) : null,
                            Setter = it.Property.CanWrite ? GetSetter(type, it.Property) : null,
                            Name = it.Column?.Name ?? it.Property.Name,
                            Convert = it.Column?.Convert,
                            Generated = it.Generated != null,
                            Key = it.Key != null,
                            Id = it.Key?.IsIdentity ?? false,
                        })
                        .ToImmutableList(),
                };
                definition.IdColumn = definition.Columns.SingleOrDefault(it => it.Id);
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
    }
}