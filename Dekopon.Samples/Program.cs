using System;
using Dekopon.Dapper;
using Dekopon.Miscs;
using Dekopon.QueryBuilder;
using Dekopon.Repository;
using Dekopon.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Dekopon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var connectionString = $@"Server=docker-host;Database=TestDb;User ID=sa;Password=yourStrong(!)Password";

            DapperInitializer.RegisterAnnotatedTypeMap(); // register typeMap for entities with [TableAttribute]

            using (var txManager = new TransactionManager())
            {
                using (var dbManager = new DatabaseManager(new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString)
                    .Options, txManager, queryBuilder: new SqlServerQueryBuilder()))
                {
                    var userRepository = new UserRepository(dbManager);
                    using (var txSupport = txManager.Begin())
                    {
                        //userRepository.FindAll();
                        //userRepository.FindAll(Enumerables.List(new UserEntity()));
                        //userRepository.Get(new UserEntity());
                        //userRepository.Add(new UserEntity());
                        //userRepository.AddAll(Enumerables.List(new UserEntity()));
                        //userRepository.Update(new UserEntity());
                        //userRepository.UpdateAll(Enumerables.List(new UserEntity()));
                        //userRepository.Delete(new UserEntity());
                        //userRepository.DeleteAll(Enumerables.List(new UserEntity()));

                        Console.WriteLine(userRepository.CountAll());
                        txSupport.Complete();
                    }
                }
            }
        }
    }
}