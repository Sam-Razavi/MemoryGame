using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetActiveGamesAsync();
        Task<int> CreateGameAsync(int userId);
        Task<int> CreateGameWithPlayersAsync(int user1Id, int user2Id); // NEW
        Task<bool> JoinGameAsync(int gameId, int userId);
        Task<Game?> GetByIdAsync(int gameId);

        Task<List<PlayerVm>> GetPlayersAsync(int gameId);
        Task<int?> GetGamePlayerIdAsync(int gameId, int userId);
        Task CompleteGameAsync(int gameId, int? winnerGamePlayerId);
        Task SetStatusAsync(int gameId, string status);
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
            const string sqlGame = @"INSERT INTO dbo.Game (Status) VALUES ('Waiting'); SELECT SCOPE_IDENTITY();";

            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                var cmdGame = new SqlCommand(sqlGame, conn, tran);
                var gameId = Convert.ToInt32(await cmdGame.ExecuteScalarAsync());

                const string sqlPlayer = @"INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder) VALUES (@GameID, @UserID, 1);";
                var cmdPlayer = new SqlCommand(sqlPlayer, conn, tran);
                cmdPlayer.Parameters.AddWithValue("@GameID", gameId);
                cmdPlayer.Parameters.AddWithValue("@UserID", userId);
                await cmdPlayer.ExecuteNonQueryAsync();

                await tran.CommitAsync();
                return gameId;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        // NEW: create a game immediately with two players and mark InProgress
        public async Task<int> CreateGameWithPlayersAsync(int user1Id, int user2Id)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                var cmdGame = new SqlCommand(
                    "INSERT INTO dbo.Game (Status) VALUES ('InProgress'); SELECT SCOPE_IDENTITY();",
                    conn, tran);
                var gameId = Convert.ToInt32(await cmdGame.ExecuteScalarAsync());

                var p1 = new SqlCommand(
                    "INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder) VALUES (@g, @u, 1);",
                    conn, tran);
                p1.Parameters.AddWithValue("@g", gameId);
                p1.Parameters.AddWithValue("@u", user1Id);
                await p1.ExecuteNonQueryAsync();

                var p2 = new SqlCommand(
                    "INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder) VALUES (@g, @u, 2);",
                    conn, tran);
                p2.Parameters.AddWithValue("@g", gameId);
                p2.Parameters.AddWithValue("@u", user2Id);
                await p2.ExecuteNonQueryAsync();

                await tran.CommitAsync();
                return gameId;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> JoinGameAsync(int gameId, int userId)
        {
            const string sqlCheck = @"SELECT COUNT(*) FROM dbo.GamePlayer WHERE GameID = @GameID;";

            using var conn = _factory.Create();
            await conn.OpenAsync();

            var cmdCount = new SqlCommand(sqlCheck, conn);
            cmdCount.Parameters.AddWithValue("@GameID", gameId);
            int count = (int)(await cmdCount.ExecuteScalarAsync() ?? 0);
            if (count >= 2) return false;

            const string sqlInsert = @"
INSERT INTO dbo.GamePlayer (GameID, UserID, PlayerOrder) VALUES (@GameID, @UserID, 2);
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

        public async Task<List<PlayerVm>> GetPlayersAsync(int gameId)
        {
            const string sql = @"
SELECT gp.GamePlayerID, gp.UserID, gp.PlayerOrder, u.Username
FROM dbo.GamePlayer gp
JOIN dbo.[User] u ON u.UserID = gp.UserID
WHERE gp.GameID = @g
ORDER BY gp.PlayerOrder;";

            var list = new List<PlayerVm>();
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new PlayerVm
                {
                    GamePlayerID = rd.GetInt32(0),
                    UserID = rd.GetInt32(1),
                    PlayerOrder = rd.GetInt32(2),
                    Username = rd.GetString(3)
                });
            }
            return list;
        }

        public async Task<int?> GetGamePlayerIdAsync(int gameId, int userId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT GamePlayerID FROM dbo.GamePlayer WHERE GameID=@g AND UserID=@u;", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            cmd.Parameters.AddWithValue("@u", userId);
            var val = await cmd.ExecuteScalarAsync();
            return val == null ? (int?)null : Convert.ToInt32(val);
        }

        public async Task CompleteGameAsync(int gameId, int? winnerGamePlayerId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();

            if (winnerGamePlayerId.HasValue)
            {
                var cmd = new SqlCommand(@"
UPDATE dbo.Game
SET Status='Completed',
    EndedAt=SYSDATETIME(),
    WinnerGamePlayerID=@w
WHERE GameID=@g;", conn);
                cmd.Parameters.AddWithValue("@w", winnerGamePlayerId.Value);
                cmd.Parameters.AddWithValue("@g", gameId);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                var cmd = new SqlCommand(@"
UPDATE dbo.Game
SET Status='Completed',
    EndedAt=SYSDATETIME(),
    WinnerGamePlayerID=NULL
WHERE GameID=@g;", conn);
                cmd.Parameters.AddWithValue("@g", gameId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task SetStatusAsync(int gameId, string status)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE dbo.Game SET Status=@s WHERE GameID=@g;", conn);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@g", gameId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
