using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;
using MemoryGame.Security;

namespace MemoryGame.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<int> CreateAsync(string username, string email, string rawPassword);
        Task<User?> ValidateAsync(string username, string rawPassword);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _factory;
        public UserRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = @"
SELECT TOP 1 UserID, Username, PasswordHash, Email, CreatedAt
FROM dbo.[User]
WHERE Username = @Username;";

            using var conn = _factory.Create();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var rd = await cmd.ExecuteReaderAsync();
            if (!rd.Read()) return null;

            return new User
            {
                UserID = rd.GetInt32(0),
                Username = rd.GetString(1),
                PasswordHash = rd.GetString(2),
                Email = rd.IsDBNull(3) ? null : rd.GetString(3),
                CreatedAt = rd.GetDateTime(4)
            };
        }

        public async Task<int> CreateAsync(string username, string email, string rawPassword)
        {
            var hash = PasswordHasher.Hash(rawPassword);

            const string sql = @"
INSERT INTO dbo.[User] (Username, PasswordHash, Email)
VALUES (@Username, @PasswordHash, @Email);
SELECT SCOPE_IDENTITY();";

            using var conn = _factory.Create();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", hash);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);

            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<User?> ValidateAsync(string username, string rawPassword)
        {
            var user = await GetByUsernameAsync(username);
            if (user is null) return null;

            return PasswordHasher.Verify(rawPassword, user.PasswordHash) ? user : null;
        }
    }
}
