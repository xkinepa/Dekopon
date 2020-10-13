using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dekopon.Entity;
using Dekopon.Miscs;

namespace Dekopon.QueryBuilder
{
    public class SqlServerQueryBuilder : IQueryBuilder, IEntityQueryBuilder, ICrudEntityQueryBuilder
    {
        private static readonly ConcurrentDictionary<(Type, string), string[]> QueryTemplates = new ConcurrentDictionary<(Type, string), string[]>();

        public virtual string Paging(string query, string orderBy = null)
        {
            return $@"select * from ({query}) as q {orderBy ?? "order by current_timestamp"} OFFSET @start ROWS FETCH NEXT @count ROWS ONLY";
        }

        public virtual string Counting(string query)
        {
            return $"select count(0) from ({query}) as q";
        }

        public string Table(EntityDefinition entityDefinition) => TableName(entityDefinition);

        public string Columns(EntityDefinition entityDefinition) => Join(entityDefinition.Columns.Select(ColumnName));

        public (string Query, ParameterContainer Params) FindAll(EntityDefinition entityDefinition)
        {
            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(FindAll)}({nameof(EntityDefinition)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var tableName = TableName(entityDefinition);
                    foreach (var columnDefinition in entityDefinition.Columns)
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add($"{tableName}.{columnName}");
                    }

                    AssertionNotEmpty(columns, $"entity has no columns");
                    return new[]
                    {
                        $"select {Join(columns)} from {tableName} where {Where(entityDefinition)};",
                    };
                });

            return (query[0], new ParameterContainer());
        }

        public (string Query, ParameterContainer Params) Find(EntityDefinition entityDefinition, object entity)
        {
            Assertion.NotNull(entity, $"{nameof(entity)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(Find)}({nameof(EntityDefinition)},{nameof(Object)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var wheres = Enumerables.List<string>();
                    var tableName = TableName(entityDefinition);
                    foreach (var columnDefinition in entityDefinition.Columns)
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add($"{tableName}.{columnName}");

                        if (columnDefinition.Key)
                        {
                            var propertyName = PropertyName(columnDefinition);
                            wheres.Add($"{ConvertText(columnName, columnDefinition.Convert)} = {CastProperty(propertyName, columnDefinition.DbType)}");
                        }
                    }

                    AssertionNotEmpty(columns, $"entity has no columns");
                    AssertionNotEmpty(wheres, $"entity has no key columns");
                    return new[]
                    {
                        $"select {Join(columns)} from {tableName} where {Where(entityDefinition)} and {Join(wheres, " and ")};",
                    };
                });

            return (query[0], BuildParameter(entityDefinition, it => it.Key, entity));
        }

        public (string Query, ParameterContainer Params) FindAll(EntityDefinition entityDefinition, IEnumerable entities)
        {
            Assertion.NotNull(entities, $"{nameof(entities)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(FindAll)}({nameof(EntityDefinition)},{nameof(IEnumerable)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var keyColumns = Enumerables.List<string>();
                    var ons = Enumerables.List<string>();
                    var tableName = TableName(entityDefinition);
                    foreach (var columnDefinition in entityDefinition.Columns)
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add($"{tableName}.{columnName}");

                        if (columnDefinition.Key)
                        {
                            keyColumns.Add(columnName);
                            ons.Add($"{ConvertText($"{tableName}.{columnName}", columnDefinition.Convert)} = data.{columnName}");
                        }
                    }

                    AssertionNotEmpty(columns, $"entity has no columns");
                    AssertionNotEmpty(ons, $"entity has no key columns");
                    return new[]
                    {
                        $@"select {Join(columns)} from {tableName} join (values ",
                        $@") as data({Join(keyColumns)}) on {Join(ons, " and ")};",
                    };
                });

            var (values, parameters) = BuildValues(entityDefinition, it => it.Key, entities);
            return ($"{query[0]}{values}{query[1]}", parameters);
        }

        public (string Query, ParameterContainer Params) Insert(EntityDefinition entityDefinition, object entity)
        {
            Assertion.NotNull(entity, $"{nameof(entity)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(Insert)}({nameof(EntityDefinition)},{nameof(Object)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var values = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns.Where(it => it.Insert))
                    {
                        var columnName = ColumnName(columnDefinition);
                        var propertyName = PropertyName(columnDefinition);
                        columns.Add(columnName);
                        values.Add(CastProperty(propertyName, columnDefinition.DbType));
                    }

                    AssertionNotEmpty(columns, $"entity has no insert columns");
                    return new[]
                    {
                        $@"insert into {TableName(entityDefinition)} ({Join(columns)}) values ({Join(values)});
select convert(bigint, scope_identity()) as [identity];",
                    };
                });

            return (query[0], BuildParameter(entityDefinition, it => it.Insert, entity));
        }

        public (string Query, ParameterContainer Params) InsertAll(EntityDefinition entityDefinition, IEnumerable entities)
        {
            Assertion.NotNull(entities, $"{nameof(entities)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(InsertAll)}({nameof(EntityDefinition)},{nameof(IEnumerable)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns.Where(it => it.Insert))
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add(columnName);
                    }

                    AssertionNotEmpty(columns, $"entity has no insert columns");
                    return new[]
                    {
                        $@"insert into {TableName(entityDefinition)} ({Join(columns)}) values ",
                        $@";"
                    };
                });

            var (values, parameters) = BuildValues(entityDefinition, it => it.Insert, entities);
            return ($"{query[0]}{values}{query[1]}", parameters);
        }

        public (string Query, ParameterContainer Params) Update(EntityDefinition entityDefinition, object entity)
        {
            Assertion.NotNull(entity, $"{nameof(entity)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(Update)}({nameof(EntityDefinition)},{nameof(entity)})"),
                _ =>
                {
                    var sets = Enumerables.List<string>();
                    var wheres = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns)
                    {
                        var columnName = ColumnName(columnDefinition);
                        var propertyName = PropertyName(columnDefinition);

                        if (columnDefinition.Key)
                        {
                            wheres.Add($"{ConvertText(columnName, columnDefinition.Convert)} = {CastProperty(propertyName, columnDefinition.DbType)}");
                        }

                        if (columnDefinition.Update)
                        {
                            sets.Add($"{columnName} = {CastProperty(propertyName, columnDefinition.DbType)}");
                        }
                    }

                    AssertionNotEmpty(sets, $"entity has no update columns");
                    AssertionNotEmpty(wheres, $"entity has no key columns");
                    return new[]
                    {
                        $"update {TableName(entityDefinition)} set {Join(sets)} where {Join(wheres, " and ")};",
                    };
                });

            return (query[0], BuildParameter(entityDefinition, it => it.Key || it.Update, entity));
        }

        public (string Query, ParameterContainer Params) UpdateAll(EntityDefinition entityDefinition, IEnumerable entities)
        {
            Assertion.NotNull(entities, $"{nameof(entities)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(UpdateAll)}({nameof(EntityDefinition)},{nameof(IEnumerable)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var sets = Enumerables.List<string>();
                    var ons = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns)
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add(columnName);

                        if (columnDefinition.Key)
                        {
                            ons.Add($"{ConvertText($"{TableName(entityDefinition)}.{columnName}", columnDefinition.Convert)} = data.{columnName}");
                        }

                        if (columnDefinition.Update)
                        {
                            sets.Add($"{columnName} = data.{columnName}");
                        }
                    }

                    AssertionNotEmpty(sets, $"entity has no update columns");
                    AssertionNotEmpty(ons, $"entity has no key columns");
                    return new[]
                    {
                        $@"update {TableName(entityDefinition)} set {Join(sets)} from {TableName(entityDefinition)} join (values ",
                        $@") as data({Join(columns)}) on {Join(ons, " and ")};",
                    };
                });

            var (values, parameters) = BuildValues(entityDefinition, it => it.Key || it.Update, entities);
            return ($"{query[0]}{values}{query[1]}", parameters);
        }

        public (string Query, ParameterContainer Params) Delete(EntityDefinition entityDefinition, object entity)
        {
            Assertion.NotNull(entity, $"{nameof(entity)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(Delete)}({nameof(EntityDefinition)},{nameof(Object)})"),
                _ =>
                {
                    var wheres = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns.Where(it => it.Key))
                    {
                        var columnName = ColumnName(columnDefinition);
                        var propertyName = PropertyName(columnDefinition);
                        wheres.Add($"{ConvertText(columnName, columnDefinition.Convert)} = {CastProperty(propertyName, columnDefinition.DbType)}");
                    }

                    AssertionNotEmpty(wheres, $"entity has no key columns");
                    return new[]
                    {
                        string.IsNullOrEmpty(entityDefinition.SetForDelete)
                            ? $@"delete from {TableName(entityDefinition)} where {Join(wheres, " and ")};"
                            : $@"update {TableName(entityDefinition)} set {entityDefinition.SetForDelete} where {Join(wheres, " and ")};",
                    };
                });

            return (query[0], BuildParameter(entityDefinition, it => it.Key, entity));
        }

        public (string Query, ParameterContainer Params) DeleteAll(EntityDefinition entityDefinition, IEnumerable entities)
        {
            Assertion.NotNull(entities, $"{nameof(entities)} should be specified");

            var query = QueryTemplates.GetOrAdd(
                (entityDefinition.Type, $"{nameof(DeleteAll)}({nameof(EntityDefinition)},{nameof(IEnumerable)})"),
                _ =>
                {
                    var columns = Enumerables.List<string>();
                    var ons = Enumerables.List<string>();
                    foreach (var columnDefinition in entityDefinition.Columns.Where(it => it.Key))
                    {
                        var columnName = ColumnName(columnDefinition);
                        columns.Add(columnName);
                        ons.Add($"{ConvertText($"{TableName(entityDefinition)}.{columnName}", columnDefinition.Convert)} = data.{columnName}");
                    }

                    AssertionNotEmpty(columns, $"entity has no columns");
                    AssertionNotEmpty(ons, $"entity has no key columns");
                    return new[]
                    {
                        string.IsNullOrEmpty(entityDefinition.SetForDelete)
                            ? $@"delete {TableName(entityDefinition)} from {TableName(entityDefinition)} join (values "
                            : $@"update {TableName(entityDefinition)} set {entityDefinition.SetForDelete} join (values ",
                        $@") as data({Join(columns)}) on {Join(ons, " and ")};",
                    };
                });

            var (values, parameters) = BuildValues(entityDefinition, it => it.Key, entities);
            return ($"{query[0]}{values}{query[1]}", parameters);
        }

        public ParameterContainer BuildParameter(EntityDefinition entityDefinition, Func<ColumnDefinition, bool> columnFilter, object entity)
        {
            var parameters = new ParameterContainer();
            foreach (var columnDefinition in entityDefinition.Columns.Where(columnFilter))
            {
                var propertyName = PropertyName(columnDefinition);
                var propertyValue = GetValue(columnDefinition, entity);
                parameters.Add(propertyName, propertyValue);
            }

            return parameters;
        }

        private (string Query, ParameterContainer Params) BuildValues(EntityDefinition entityDefinition, Func<ColumnDefinition, bool> columnFilter, IEnumerable entities)
        {
            var i = 0;
            var valuesList = Enumerables.List<string>();
            var parameters = new ParameterContainer();
            foreach (var entity in entities)
            {
                var index = i++;
                var values = Enumerables.List<string>();
                foreach (var columnDefinition in entityDefinition.Columns.Where(columnFilter))
                {
                    var propertyName = PropertyName(columnDefinition, index);
                    var propertyValue = GetValue(columnDefinition, entity);
                    values.Add(propertyName);
                    parameters.Add(propertyName, propertyValue);
                }

                valuesList.Add($"({Join(values)})");
            }

            Assertion.IsTrue(i != 0, $"entities should be specified");
            return (Join(valuesList), parameters);
        }

        public string Where(EntityDefinition entity)
        {
            if (!string.IsNullOrEmpty(entity.Where))
            {
                return entity.Where;
            }

            return $"0 = 0";
        }

        private string TableName(EntityDefinition entity)
        {
            return $"[{entity.Table}]";
        }

        private string TempTableName(EntityDefinition entity)
        {
            return $"[#{entity.Table}]";
        }

        private string PropertyName(ColumnDefinition column)
        {
            return $"@{column.Property.Name}";
        }

        private string PropertyName(ColumnDefinition column, int index)
        {
            return $"@{column.Property.Name}_{index}";
        }

        private string ColumnName(ColumnDefinition column)
        {
            return $"[{column.Name}]";
        }

        private string ConvertText(string text, string convert)
        {
            if (!string.IsNullOrEmpty(convert))
            {
                return string.Format(convert, text);
            }

            return text;
        }

        private string CastProperty(string property, string dbType)
        {
            if (!string.IsNullOrEmpty(dbType))
            {
                return $"cast({property} as {dbType})";
            }

            return property;
        }

        private object GetValue(ColumnDefinition column, object entity)
        {
            return (entity != null && column.Getter != null) ? column.Getter.Invoke(entity) : DBNull.Value;
        }

        private string Join(IEnumerable<string> tokens, string separator = ", ")
        {
            return string.Join(separator, tokens);
        }

        private void AssertionNotEmpty(IEnumerable collection, string message)
        {
            var empty = true;
            foreach (var _ in collection)
            {
                empty = false;
                break;
            }

            Assertion.IsTrue(!empty, message);
        }
    }

    public class SqlServer08QueryBuilder : SqlServerQueryBuilder
    {
        public override string Paging(string query, string orderBy = null)
        {
            return $@"select * from (
    select *, row_number() over ({orderBy ?? "order by current_timestamp"}) as _rn from (
        {query} 
    ) as q
) as q where _rn between @start + 1 and @start + @count order by _rn asc";
        }
    }
}