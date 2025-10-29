using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetActiveGamesAsync();
        Task<int> CreateGameAsync(int userId);
        Task<bool> JoinGameAsync(int gameId, int userId);
        Task<Game?> GetByIdAsync(int gameId);
    }

    public class GameRepository : IGameRepository
    {
        private readonly IDbConnectionFactory _factory;
        public GameRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<Game>> GetActiveGamesAsync()
        {
            const string sql = "SELECT GameID, CreatedAt, EndedAt, Status, WinnerGamePlayerID FROM dbo.Game ORDER BY CreatedAt DESC";
            var list = new List<Game>();

            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new Game
                {
                    GameID = rd.GetInt32(0),
                    CreatedAt = rd.GetDateTime(1),
                    EndedAt = rd.IsDBNull(2) ? null : rd.GetDateTime(2),
                    Status = rd.GetString(3),
                    WinnerGamePlayerID = rd.IsDBNull(4) ? null : rd.GetInt32(4)
                });
            }
            return list;
        }

        public async Task<int> CreateGameAsync(int userId)
        {
            const string sqlGame = @"
INSERT INTO dbo.Game (Status) VALUES ('Waiting');
SELECT SCOPE_IDENTITY();";

            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                var cmdGame = new SqlCommand(sqlGame, conn, tran);
                var gameId = Convert.ToInt32(await cmdGame.ExecuteScalarAsync());

                const string sqlPlayer = @"
INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder)
VALUES (@GameID, @UserID, 1);";
                var cmdPlayer = new SqlCommand(sqlPlayer, conn, tran);
                cmdPlayer.Parameters.AddWithValue("@GameID", gameId);
                cmdPlayer.Parameters.AddWithValue("@UserID", userId);
                await cmdPlayer.ExecuteNonQueryAsync();

                tran.Commit();
                return gameId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> JoinGameAsync(int gameId, int userId)
        {
            const string sqlCheck = @"
SELECT COUNT(*) FROM dbo.GamePlayer WHERE GameID = @GameID;";

            using var conn = _factory.Create();
            await conn.OpenAsync();

            var cmdCount = new SqlCommand(sqlCheck, conn);
            cmdCount.Parameters.AddWithValue("@GameID", gameId);
            int count = (int)await cmdCount.ExecuteScalarAsync();
            if (count >= 2) return false; // already full

            const string sqlInsert = @"
INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder)
VALUES (@GameID, @UserID, 2);
UPDATE dbo.Game SET Status = 'InProgress' WHERE GameID = @GameID;";
            var cmd = new SqlCommand(sqlInsert, conn);
            cmd.Parameters.AddWithValue("@GameID", gameId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            await cmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<Game?> GetByIdAsync(int gameId)
        {
            const string sql = "SELECT GameID, CreatedAt, EndedAt, Status, WinnerGamePlayerID FROM dbo.Game WHERE GameID=@GameID;";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GameID", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!rd.Read()) return null;
            return new Game
            {
                GameID = rd.GetInt32(0),
                CreatedAt = rd.GetDateTime(1),
                EndedAt = rd.IsDBNull(2) ? null : rd.GetDateTime(2),
                Status = rd.GetString(3),
                WinnerGamePlayerID = rd.IsDBNull(4) ? null : rd.GetInt32(4)
            };
        }
    }
}
