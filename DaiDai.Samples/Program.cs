using System;
using DaiDai.Dapper;
using DaiDai.Miscs;
using DaiDai.QueryBuilder;
using DaiDai.Repository;
using DaiDai.Transaction;
using Microsoft.EntityFrameworkCore;

namespace DaiDai
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var connectionString = $@"Server=(local);Database=tempdb;User ID=sa;Password=sa";

            DapperInitializer.RegisterAnnotatedTypeMap(); // register typeMap for entities with [TableAttribute]

            using (var txManager = new TransactionManager())
            {
                using (var dbManager = new DatabaseManager(new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString)
                    .Options, txManager, entityQueryBuilder: new SqlServerEntityQueryBuilder()))
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