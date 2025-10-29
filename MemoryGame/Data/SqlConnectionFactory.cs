using Microsoft.Data.SqlClient;

namespace MemoryGame.Data
{
    public interface IDbConnectionFactory
    {
        SqlConnection Create();
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connStr;

        public SqlConnectionFactory(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("MemoryGameDb")
                ?? throw new InvalidOperationException("Connection string 'MemoryGameDb' not found.");
        }

        public SqlConnection Create() => new SqlConnection(_connStr);
    }
}
