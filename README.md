# DaiDai

[![NuGet](https://img.shields.io/nuget/v/DaiDai.Repository.svg)](https://www.nuget.org/packages/DaiDai.Repository)
[![NuGet](https://img.shields.io/nuget/dt/DaiDai.Repository.svg)](https://www.nuget.org/packages/DaiDai.Repository)

[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT) 

[![GitHub stars](https://img.shields.io/github/stars/xkinepa/DaiDai.svg?style=social&label=Star)](https://github.com/xkinepa/DaiDai)
[![GitHub forks](https://img.shields.io/github/forks/xkinepa/DaiDai.svg?style=social&label=Fork)](https://github.com/xkinepa/DaiDai)
[![GitHub watchers](https://img.shields.io/github/watchers/xkinepa/DaiDai.svg?style=social&label=Watch)](https://github.com/xkinepa/DaiDai)

DaiDai is a repository-style data access layer implementation, supports handy transaction management and batch insert/update.

## Basics

DaiDai currently only targets netstandard2.0, and only `SqlServerEntityQueryBuilder` is supported for now.

`IEntityQueryBuilder` generates queries for:
```csharp
    (string, IDictionary<string, object>) FindAll(EntityDefinition entityDefinition);
    (string, IDictionary<string, object>) Find(EntityDefinition entityDefinition, object entity);
    (string, IDictionary<string, object>) FindAll(EntityDefinition entityDefinition, IEnumerable entities);
    (string, IDictionary<string, object>) Insert(EntityDefinition entityDefinition, object entity);
    (string, IDictionary<string, object>) InsertAll(EntityDefinition entityDefinition, IEnumerable entities);
    (string, IDictionary<string, object>) Update(EntityDefinition entityDefinition, object entity);
    (string, IDictionary<string, object>) UpdateAll(EntityDefinition entityDefinition, IEnumerable entities);
    (string, IDictionary<string, object>) Delete(EntityDefinition entityDefinition, object entity);
    (string, IDictionary<string, object>) DeleteAll(EntityDefinition entityDefinition, IEnumerable entities);
```

The return tuple contains query and parameters, which can be passed to Dapper query and execution methods directly.

## Usages

### NuGet
`Install-Package DaiDai.Repository`

### Entity
```csharp
    [Table("Users")]
    public class UserEntity
    {
        [Key(IsIdentity = true), Generated]
        public long Id { get; set; }
        public string Username { get; set; }
        //...
        public int Deleted { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
```

### Repository
```csharp
    public class UserRepository : CrudRepositoryBase<UserEntity>, IUserRepository
    {
        public UserRepository(IDatabaseManager databaseManager) : base(databaseManager)
        {
        }

        // you can add other queries below
        public long CountAll()
        {
            return Conn.ExecuteScalar<long>($"select count(0) from {TableName} where deleted = 0");
        }
    }

    using (var dbManager = new DatabaseManager(new DbContextOptionsBuilder()
        .UseSqlServer(connectionString)
        .Options, entityQueryBuilder: new SqlServerEntityQueryBuilder()))
    {
        var userRepository = new UserRepository(dbManager);
        userRepository.Add(new UserEntity());
    }
```

`ICrudRepository<T>` contains below methods:
```csharp
    public interface ICrudRepository<T> : IRepository<T>
    {
        IList<T> FindAll(IList<T> entities);
        T Get(T entity);

        long Add(T entity);
        int AddAll(IList<T> entities, int chunk = 100);

        int Update(T entity);
        int UpdateAll(IList<T> entities, int chunk = 100);

        int Delete(T entity);
        int DeleteAll(IList<T> entities);

        IList<T> FindByIdIn(IList<long> ids);
        T GetById(long id);
        int DeleteById(long id);
        int DeleteByIdIn(IList<long> ids);
    }
```

> You can create `XyzRepository` from non-generic `RepositoryBase` without entities.

### Transaction and Connection lifecycle
`TransactionManager` simply wraps `TransactionScope`.
When `RepositoryBase.Conn` or `IDatabaseManager.GetConnection()` is invoked, DaiDai will check if `Transaction.Current` exists:
* If no transaction exists, a new connection will be opened/reused and live along with the `IDatabaseManager`;
* If transaction exists, a new connection will be opened/reused and live along with the `transaction.TransactionCompleted`.

You can create nested txSupport with different isolation and propagation parameters.
See `TransactionAwareResourceManager` for details.

So, the order of `new UserRepository()` and `txManager.Begin()` doesn't matter, you can define your custom [TransactionalAttribute] with AOP frameworks.

```csharp
    using (var txManager = new TransactionManager()) // txManager should be singleton
    using (var dbManager = new DatabaseManager(new DbContextOptionsBuilder()
        .UseSqlServer(connectionString)
        .Options, txManager, entityQueryBuilder: new SqlServerEntityQueryBuilder()))
    {
        var userRepository = new UserRepository(dbManager);
        using (var txSupport = txManager.Begin())
        {
            Console.WriteLine(userRepository.CountAll());

            txSupport.Complete();
        }
    }
```

### IoC
If you use IoC containers like `Microsoft.Extensions.DependencyInjection` or `Autofac`, here's the best practise:
* Create a singleton `TransactionManager`;
* Create each `DatabaseManager` per http request and let it disposed at the end of the request;
* Create repositories and acquire transactions per usage, in your business logic layer.

```csharp
    // Autofac
    builder.RegisterType<TransactionManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
    builder.Register(c => new DatabaseManager(new DbContextOptionsBuilder()
            .UseSqlServer(ConnectionString)
            .Options, c.Resolve<ITransactionManager>(), c.Resolve<SqlServerEntityQueryBuilder>()
    )).AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();

    builder.RegisterType<SqlServerEntityQueryBuilder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
    builder.RegisterType<UserRepository>().AsSelf().AsImplementedInterfaces().InstancePerDependency();

    builder.RegisterType<UserService>().AsSelf().AsImplementedInterfaces().InstancePerDependency();

    //
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionManager _txManager;

        public UserService(IUserRepository userRepository, ITransactionManager txManager)
        {
            _userRepository = userRepository;
			_txManager = txManager;
        }

		public void CreateUser()
		{
			using (var txSupport = _txManager.Begin(TransactionScopeOption.Required, IsolationLevel.ReadCommitted))
			{
				userRepository.Add(new UserEntity { });

				txSupport.Complete();
			}
		}
    }
```

### Suggestions
* Sql Server limit 2100 parameters in one command, so batch methods like `AddAll` and `UpdateAll` accepts chunk size as a parameter, suggested value is `2100 / fieldCountInEntityT`.
* If you need any wrap over connections, for example working with `ProfiledDbConnection` from `MiniProfiler`, simply derive `DatabaseManager` or `SqlConnectionManager`.
* Fell free to implement `IEntityQueryBuilder` for other databases, any PR are welcome.
* See `DaiDai.Samples` for full example.

## Entities and Queries

### Attributes on entities
* `[TableAttribute]` marks entities and suggests database table name;
* `[KeyAttribute]` marks key columns which will be used in `Find`, `Update` and `Delete` methods;
* `[KeyAttribute].IsIdentity` indicates the column is identity and will be updated after `Insert` (not `InsertAll`), **one entity can only has one `IsIdentity=true` property and its type must be `long`**;
* `[GeneratedAttribute]` marks the key is ignored from `Insert`;
* `[WhereAttribute]` affects `Find` methods;
* `[DeleteAttribute].Set` indicates the entity will be **UPDATE**d with the `Set` clause rather than hard **DELETE**.
* `[ConvertAttribute]` indicates when the column is also marked as `[Key]`, the `[ConvertAttribute].Pattern` will be applied to this column when join and compare, which is useful when the database column is ntext (cannot be used to =) or datetime (loses timezone info hence cannot compare to DateTimeOffset) or in other circumstances. **Be aware this will invalidate database indices.**

### Queries

```csharp
    [Table("Users")]
    [Where(Clause = "deleted = 0")]
    [Delete(Set = "deleted = 1")]
    public class UserEntity
    {
        [Key(IsIdentity = true), Generated]
        public long Id { get; set; }
        
        public string Username { get; set; }
        
        public int Deleted { get; set; }
        
        //[Column("CreateTime"), Convert("TODATETIMEOFFSET({0}, DATEPART(tz, SYSDATETIMEOFFSET()))")]
        public DateTimeOffset CreateTime { get; set; }
    }
```

For UserEntity defined above, all queries are generated as:

#### SqlServerEntityQueryBuilder

##### FindAll
```sql
select [Users].[Id], [Users].[Username], [Users].[Deleted], [Users].[CreateTime] from [Users] where deleted = 0;
```

##### FindAll
```sql
select [Users].[Id], [Users].[Username], [Users].[Deleted], [Users].[CreateTime] from [Users] join (values (@Id_0)) as data([Id]) on [Users].[Id] = data.[Id];
```

##### Get
```sql
select [Users].[Id], [Users].[Username], [Users].[Deleted], [Users].[CreateTime] from [Users] where deleted = 0 and [Id] = @Id;
```

##### Add
```sql
insert into [Users] ([Username], [Deleted], [CreateTime]) values (@Username, @Deleted, @CreateTime);
select convert(bigint, scope_identity()) as [identity];
```

##### AddAll
```sql
insert into [Users] ([Username], [Deleted], [CreateTime]) values (@Username_0, @Deleted_0, @CreateTime_0);
```

##### Update
```sql
update [Users] set [Username] = @Username, [Deleted] = @Deleted, [CreateTime] = @CreateTime where [Id] = @Id;
```

##### UpdateAll
```sql
update [Users] set [Username] = data.[Username], [Deleted] = data.[Deleted], [CreateTime] = data.[CreateTime] from [Users] join (values (@Id_0, @Username_0, @Deleted_0, @CreateTime_0)) as data([Id], [Username], [Deleted], [CreateTime]) on [Users].[Id] = data.[Id];
```

##### Delete
```sql
update [Users] set deleted = 1 where [Id] = @Id;
```

##### DeleteAll
```sql
update [Users] set deleted = 1 join (values (@Id_0)) as data([Id]) on [Users].[Id] = data.[Id];
```

## TODO
* [x] Write usages
* [x] Write tests (more tests needed)
* [x] Publish to NuGet (pre-release ver)
