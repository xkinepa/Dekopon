using Dapper;

namespace Daidai.Repository
{
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

    public interface IUserRepository : ICrudRepository<UserEntity>
    {
        long CountAll();
    }
}