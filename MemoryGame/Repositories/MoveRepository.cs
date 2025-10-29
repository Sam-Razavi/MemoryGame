using Microsoft.Data.SqlClient;
using MemoryGame.Data;

namespace MemoryGame.Repositories
{
    public interface IMoveRepository
    {
        Task<int> GetNextTurnAsync(int gameId);
        Task<int?> GetLastMoverUserIdAsync(int gameId);
        Task RecordAsync(int gameId, int userId, int firstTileId, int secondTileId, bool isMatch);
    }

    public class MoveRepository : IMoveRepository
    {
        private readonly IDbConnectionFactory _factory;
        public MoveRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<int> GetNextTurnAsync(int gameId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT ISNULL(MAX(TurnNumber),0)+1 FROM dbo.Move WHERE GameID=@g", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int?> GetLastMoverUserIdAsync(int gameId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(
                "SELECT TOP 1 UserID FROM dbo.Move WHERE GameID=@g ORDER BY TurnNumber DESC", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            var val = await cmd.ExecuteScalarAsync();
            return val == null ? null : Convert.ToInt32(val);
        }

        public async Task RecordAsync(int gameId, int userId, int t1, int t2, bool isMatch)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
INSERT INTO dbo.Move (GameID, UserID, FirstTileID, SecondTileID, IsMatch, TurnNumber)
SELECT @g, @u, @t1, @t2, @m, ISNULL(MAX(TurnNumber),0)+1
FROM dbo.Move WHERE GameID=@g;", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@t1", t1);
            cmd.Parameters.AddWithValue("@t2", t2);
            cmd.Parameters.AddWithValue("@m", isMatch);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
