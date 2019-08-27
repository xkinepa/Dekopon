using System;
using DaiDai.Repository;
using DaiDai.Transaction;
using Microsoft.EntityFrameworkCore;

namespace DaiDai
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var connectionString = $@"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=sa";
            using (var txManager = new TransactionManager())
            {
                using (var dbManager = new DatabaseManager(new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString)
                    .Options, txManager))
                {
                    var userRepository = new UserRepository(dbManager);
                    using (var txSupport = txManager.Begin())
                    {
                        Console.WriteLine(userRepository.CountAll());

                        txSupport.Complete();
                    }
                }
            }
        }
    }
}