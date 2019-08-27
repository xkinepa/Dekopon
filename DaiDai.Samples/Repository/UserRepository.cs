using Dapper;

namespace DaiDai.Repository
{
    public class UserRepository : CrudRepositoryBase<UserEntity>
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
}