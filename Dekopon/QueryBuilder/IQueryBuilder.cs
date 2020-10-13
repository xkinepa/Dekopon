using System.Collections;
using System.Collections.Generic;
using Dekopon.Entity;

namespace Dekopon.QueryBuilder
{
    public interface IQueryBuilder
    {
        string Paging(string query, string orderBy = null);

        string Counting(string query);
    }

    public interface IEntityQueryBuilder : IQueryBuilder
    {
        string Table(EntityDefinition entityDefinition);
        string Columns(EntityDefinition entityDefinition);
        string Where(EntityDefinition entityDefinition);

        (string Query, ParameterContainer Params) FindAll(EntityDefinition entityDefinition);
    }

    public interface ICrudEntityQueryBuilder : IEntityQueryBuilder
    {
        (string Query, ParameterContainer Params) Find(EntityDefinition entityDefinition, object entity);
        (string Query, ParameterContainer Params) FindAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string Query, ParameterContainer Params) Insert(EntityDefinition entityDefinition, object entity);
        (string Query, ParameterContainer Params) InsertAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string Query, ParameterContainer Params) Update(EntityDefinition entityDefinition, object entity);
        (string Query, ParameterContainer Params) UpdateAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string Query, ParameterContainer Params) Delete(EntityDefinition entityDefinition, object entity);
        (string Query, ParameterContainer Params) DeleteAll(EntityDefinition entityDefinition, IEnumerable entities);
    }

    public class ParameterContainer : Dictionary<string, object>
    {
    }
}