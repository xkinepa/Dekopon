using System.Collections;
using System.Collections.Generic;
using Dekopon.Entity;

namespace Dekopon.QueryBuilder
{
    public interface IEntityQueryBuilder
    {
        string Table(EntityDefinition entityDefinition);
        string Columns(EntityDefinition entityDefinition);

        (string, IDictionary<string, object>) FindAll(EntityDefinition entityDefinition);
        (string, IDictionary<string, object>) Find(EntityDefinition entityDefinition, object entity);
        (string, IDictionary<string, object>) FindAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string, IDictionary<string, object>) Insert(EntityDefinition entityDefinition, object entity);
        (string, IDictionary<string, object>) InsertAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string, IDictionary<string, object>) Update(EntityDefinition entityDefinition, object entity);
        (string, IDictionary<string, object>) UpdateAll(EntityDefinition entityDefinition, IEnumerable entities);
        (string, IDictionary<string, object>) Delete(EntityDefinition entityDefinition, object entity);
        (string, IDictionary<string, object>) DeleteAll(EntityDefinition entityDefinition, IEnumerable entities);
    }
}