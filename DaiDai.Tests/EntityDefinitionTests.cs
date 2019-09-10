using System;
using System.Linq;
using System.Reflection;
using Daidai.Attributes;
using Daidai.Entity;
using Daidai.Miscs;
using Xunit;

namespace Daidai
{
    public class EntityDefinitionTests : IDisposable
    {
        private readonly EntityDefinitionContainer _entityDefinitionContainer;

        public EntityDefinitionTests()
        {
            _entityDefinitionContainer = EntityDefinitionContainer.Instance;
        }

        public void Dispose()
        {
        }

        [Fact]
        public void AcceptableEntityClass()
        {
            Assert.NotNull(_entityDefinitionContainer.Get(typeof(EntityWithMinimumAttributes)));
            Assert.Throws<AssertionException>(() => _entityDefinitionContainer.Get(typeof(EntityWithoutAttributes)));
        }

        [Fact]
        public void MinimumAttributes()
        {
            var type = typeof(EntityWithMinimumAttributes);
            var entityDefinition = _entityDefinitionContainer.Get(type);
            Assert.NotNull(entityDefinition);

            Assert.Null(entityDefinition.IdColumn);
            Assert.Equal(type, entityDefinition.Type);
            Assert.Equal(nameof(EntityWithMinimumAttributes), entityDefinition.Table);
            Assert.Equal(type.GetRuntimeProperties().Count(), entityDefinition.Columns.Count);
            Assert.Contains(entityDefinition.Columns, it => string.Equals(it.Name, nameof(EntityWithMinimumAttributes.Id)));
            Assert.Contains(entityDefinition.Columns, it => string.Equals(it.Name, nameof(EntityWithMinimumAttributes.Name)));
        }

        [Fact]
        public void AllAttributes()
        {
            var type = typeof(EntityWithAllAttributes);
            var entityDefinition = _entityDefinitionContainer.Get(type);
            Assert.NotNull(entityDefinition);

            Assert.NotEmpty(entityDefinition.Table);
            Assert.NotEqual(nameof(EntityWithAllAttributes), entityDefinition.Table);

            Assert.NotEmpty(entityDefinition.Where);
            Assert.NotEmpty(entityDefinition.SetForDelete);

            var idColumn = entityDefinition.IdColumn;
            Assert.NotNull(idColumn);
            Assert.Equal(type.GetRuntimeProperty(nameof(EntityWithAllAttributes.Id)), idColumn.Property);
            Assert.NotEqual(nameof(EntityWithAllAttributes.Id), idColumn.Name);
            Assert.True(idColumn.Generated);
            Assert.True(idColumn.Key);
            Assert.True(idColumn.Id);
            Assert.False(idColumn.Insert);
            Assert.False(idColumn.Update);
        }

        [Fact]
        public void ColumnSetter()
        {
            var type = typeof(EntityWithAllAttributes);
            var entityDefinition = _entityDefinitionContainer.Get(type);
            var idColumn = entityDefinition.IdColumn;
            var entity = new EntityWithAllAttributes();
            Assert.Equal(0L, entity.Id);
            idColumn.Setter.Invoke(entity, 1L);
            Assert.Equal(1L, entity.Id);
            idColumn.Setter.Invoke(entity, 0L);
            Assert.Equal(0L, entity.Id);

            var nameColumn = entityDefinition.Columns.Single(it => string.Equals(it.Property.Name, nameof(EntityWithAllAttributes.Name)));
            Assert.Null(entity.Name);
            nameColumn.Setter.Invoke(entity, "test");
            Assert.Equal("test", entity.Name);
            nameColumn.Setter.Invoke(entity, null);
            Assert.Null(entity.Name);
        }

        [Fact]
        public void ColumnGetter()
        {
            var type = typeof(EntityWithAllAttributes);
            var entityDefinition = _entityDefinitionContainer.Get(type);
            var idColumn = entityDefinition.IdColumn;
            var entity = new EntityWithAllAttributes();
            Assert.Equal(0L, idColumn.Getter.Invoke(entity));
            entity.Id = 1L;
            Assert.Equal(1L, idColumn.Getter.Invoke(entity));
            entity.Id = 0L;
            Assert.Equal(0L, idColumn.Getter.Invoke(entity));

            var nameColumn = entityDefinition.Columns.Single(it => string.Equals(it.Property.Name, nameof(EntityWithAllAttributes.Name)));
            Assert.Null(nameColumn.Getter.Invoke(entity));
            entity.Name = "test";
            Assert.Equal("test", nameColumn.Getter.Invoke(entity));
            entity.Name = null;
            Assert.Null(nameColumn.Getter.Invoke(entity));
        }

        public class EntityWithoutAttributes
        {
            public long Id { get; set; }
        }

        [Table]
        public class EntityWithMinimumAttributes
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        [Table("Entity0"), Where(Clause = "Status3 = 1"), Delete(Set = "Status3 = 0")]
        public class EntityWithAllAttributes
        {
            [Key(IsIdentity = true), Generated, Column("Id1")] public long Id { get; set; }
            [Column("Name2")] public string Name { get; set; }
            [Column("Status3")] public int Status { get; set; }
            [Column("CreateTime4"), Generated] public DateTimeOffset CreateTime { get; set; }
        }

        public abstract class EntityBase
        {
            [Key(IsIdentity = true), Generated, Column("Id")]
            public long Id { get; set; }
        }

        public class EntityDerived : EntityBase
        {
            public string Name { get; set; }
        }
    }
}