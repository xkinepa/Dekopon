using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dekopon.Miscs;
using Dapper;
using Dekopon.QueryBuilder;

namespace Dekopon.Repository
{
    public abstract class CrudRepositoryBase<T> : RepositoryBase<T>, ICrudRepository<T>
    {
        static CrudRepositoryBase()
        {
            var type = typeof(T);
            Assertion.IsTrue(!type.IsArray);
            Assertion.IsTrue(!type.IsGenericType || !type.GetTypeInfo().ImplementedInterfaces.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        protected ICrudEntityQueryBuilder CrudQueryBuilder => (ICrudEntityQueryBuilder) QueryBuilder;

        protected CrudRepositoryBase(IDatabaseManager databaseManager) : base(databaseManager)
        {
        }

        public virtual IList<T> FindAll(IList<T> entities)
        {
            var (query, @params) = CrudQueryBuilder.FindAll(EntityDefinition, entities);
            return Conn.Query<T>(query, @params).ToList();
        }

        public virtual T Get(T entity)
        {
            var (query, @params) = CrudQueryBuilder.Find(EntityDefinition, entity);
            return Conn.QuerySingleOrDefault<T>(query, @params);
        }

        public virtual long Add(T entity)
        {
            var (query, @params) = CrudQueryBuilder.Insert(EntityDefinition, entity);
            var id = Conn.ExecuteScalar<long>(query, @params);
            if (id > 0)
            {
                EntityDefinition.IdColumn?.Setter(entity, id);
            }

            return id;
        }

        public virtual int AddAll(IList<T> entities, int chunk = 100)
        {
            return Chunk(entities, chunk).Select(it =>
            {
                var (query, @params) = CrudQueryBuilder.InsertAll(EntityDefinition, it);
                return Conn.Execute(query, @params);
            }).Sum();
        }

        public virtual int Update(T entity)
        {
            var (query, @params) = CrudQueryBuilder.Update(EntityDefinition, entity);
            return Conn.Execute(query, @params);
        }

        public virtual int UpdateAll(IList<T> entities, int chunk = 100)
        {
            return Chunk(entities, chunk).Select(it =>
            {
                var (query, @params) = CrudQueryBuilder.UpdateAll(EntityDefinition, it);
                return Conn.Execute(query, @params);
            }).Sum();
        }

        public virtual int Delete(T entity)
        {
            var (query, @params) = CrudQueryBuilder.Delete(EntityDefinition, entity);
            return Conn.Execute(query, @params);
        }

        public virtual int DeleteAll(IList<T> entities)
        {
            var (query, @params) = CrudQueryBuilder.DeleteAll(EntityDefinition, entities);
            return Conn.Execute(query, @params);
        }

        public virtual IList<T> FindByIdIn(IList<long> ids) => FindAll(ids.Select(CreateEntity).ToList());

        public virtual T Get(long id) => GetById(id);

        public virtual T GetById(long id) => Get(CreateEntity(id));

        public virtual int DeleteById(long id) => Delete(CreateEntity(id));

        public virtual int DeleteByIdIn(IList<long> ids) => DeleteAll(ids.Select(CreateEntity).ToList());

        private T CreateEntity(long id)
        {
            Assertion.NotNull(EntityDefinition.IdColumn, $"entity has no id column");

            var entity = Activator.CreateInstance<T>();
            EntityDefinition.IdColumn.Setter(entity, id);
            return entity;
        }

        private IEnumerable<IEnumerable<TV>> Chunk<TV>(IEnumerable<TV> data, int chunk)
        {
            return data.Select((it, i) => new {it, g = i / chunk,})
                .GroupBy(it => it.g, it => it.it);
        }
    }
}