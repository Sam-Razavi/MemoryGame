using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public record LastMove(int TurnNumber, int UserID, bool IsMatch);

    public interface IMoveRepository
    {
        Task<int> GetNextTurnAsync(int gameId);
        Task<int?> GetLastMoverUserIdAsync(int gameId);
        Task RecordAsync(int gameId, int userId, int firstTileId, int secondTileId, bool isMatch);

        Task<LastMove?> GetLastMoveAsync(int gameId);
        Task<Dictionary<int, int>> GetScoresAsync(int gameId); // UserID -> pairs
        Task<List<MoveVm>> GetHistoryAsync(int gameId);
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
            var val = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(val ?? 0);
        }

        public async Task<int?> GetLastMoverUserIdAsync(int gameId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT TOP 1 UserID FROM dbo.Move WHERE GameID=@g ORDER BY TurnNumber DESC", conn);
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

        public async Task<LastMove?> GetLastMoveAsync(int gameId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
SELECT TOP 1 TurnNumber, UserID, IsMatch
FROM dbo.Move
WHERE GameID=@g
ORDER BY TurnNumber DESC;", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!rd.Read()) return null;
            return new LastMove(
                rd.GetInt32(0),
                rd.GetInt32(1),
                rd.GetBoolean(2)
            );
        }

        public async Task<Dictionary<int, int>> GetScoresAsync(int gameId)
        {
            var result = new Dictionary<int, int>();
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
SELECT UserID, COUNT(*) AS Pairs
FROM dbo.Move
WHERE GameID=@g AND IsMatch=1
GROUP BY UserID;", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result[rd.GetInt32(0)] = rd.GetInt32(1);
            }
            return result;
        }

        public async Task<List<MoveVm>> GetHistoryAsync(int gameId)
        {
            const string sql = @"
SELECT m.TurnNumber, m.UserID, u.Username, m.IsMatch, m.CreatedAt
FROM dbo.Move m
JOIN dbo.[User] u ON u.UserID = m.UserID
WHERE m.GameID = @g
ORDER BY m.TurnNumber;";
            var list = new List<MoveVm>();
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new MoveVm
                {
                    TurnNumber = rd.GetInt32(0),
                    UserID = rd.GetInt32(1),
                    Username = rd.GetString(2),
                    IsMatch = rd.GetBoolean(3),
                    CreatedAt = rd.GetDateTime(4)
                });
            }
            return list;
        }
    }
}
