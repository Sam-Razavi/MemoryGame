using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public interface ITileRepository
    {
        Task<List<Tile>> GetByGameAsync(int gameId);
        Task<bool> AnyForGameAsync(int gameId);
        Task CreateForGameAsync(int gameId, int tileCount);
        Task<Tile?> GetAsync(int tileId);
        Task<string?> GetPairKeyAsync(int tileId);
        Task SetMatchedAsync(int t1, int t2);
    }

    public class TileRepository : ITileRepository
    {
        private readonly IDbConnectionFactory _factory;
        public TileRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<bool> AnyForGameAsync(int gameId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT TOP 1 1 FROM dbo.Tile WHERE GameID=@g", conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            return (await cmd.ExecuteScalarAsync()) is not null;
        }

        public async Task<List<Tile>> GetByGameAsync(int gameId)
        {
            const string sql = @"
SELECT t.TileID, t.GameID, t.CardID, t.Position, t.IsMatched, c.Name, c.ImagePath
FROM dbo.Tile t
JOIN dbo.Card c ON c.CardID = t.CardID
WHERE t.GameID = @g
ORDER BY t.Position;";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@g", gameId);
            using var rd = await cmd.ExecuteReaderAsync();
            var list = new List<Tile>();
            while (await rd.ReadAsync())
            {
                list.Add(new Tile
                {
                    TileID = rd.GetInt32(0),
                    GameID = rd.GetInt32(1),
                    CardID = rd.GetInt32(2),
                    Position = rd.GetInt32(3),
                    IsMatched = rd.GetBoolean(4),
                    CardName = rd.GetString(5),
                    ImagePath = rd.IsDBNull(6) ? null : rd.GetString(6)
                });
            }
            return list;
        }

        public async Task CreateForGameAsync(int gameId, int tileCount)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();

            // Grab exactly tileCount card rows at random (we seeded 16 = 8 pairs)
            var cmdCards = new SqlCommand("SELECT TOP (@n) CardID FROM dbo.Card ORDER BY NEWID()", conn);
            cmdCards.Parameters.AddWithValue("@n", tileCount);
            var ids = new List<int>();
            using (var rd = await cmdCards.ExecuteReaderAsync())
                while (await rd.ReadAsync()) ids.Add(rd.GetInt32(0));

            // Shuffle positions 0..tileCount-1
            var positions = Enumerable.Range(0, tileCount).OrderBy(_ => Guid.NewGuid()).ToArray();

            using var tran = conn.BeginTransaction();
            try
            {
                for (int i = 0; i < tileCount; i++)
                {
                    var ins = new SqlCommand(
                        "INSERT INTO dbo.Tile (GameID, CardID, Position, IsMatched) VALUES (@g,@c,@p,0)",
                        conn, tran);
                    ins.Parameters.AddWithValue("@g", gameId);
                    ins.Parameters.AddWithValue("@c", ids[i]);
                    ins.Parameters.AddWithValue("@p", positions[i]);
                    await ins.ExecuteNonQueryAsync();
                }
                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<Tile?> GetAsync(int tileId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(
                "SELECT TileID, GameID, CardID, Position, IsMatched FROM dbo.Tile WHERE TileID=@t", conn);
            cmd.Parameters.AddWithValue("@t", tileId);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!rd.Read()) return null;
            return new Tile
            {
                TileID = rd.GetInt32(0),
                GameID = rd.GetInt32(1),
                CardID = rd.GetInt32(2),
                Position = rd.GetInt32(3),
                IsMatched = rd.GetBoolean(4)
            };
        }

        public async Task<string?> GetPairKeyAsync(int tileId)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
SELECT c.PairKey
FROM dbo.Tile t
JOIN dbo.Card c ON c.CardID = t.CardID
WHERE t.TileID = @t;", conn);
            cmd.Parameters.AddWithValue("@t", tileId);
            var val = await cmd.ExecuteScalarAsync();
            return val?.ToString();
        }

        public async Task SetMatchedAsync(int t1, int t2)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            var cmd = new SqlCommand(
                "UPDATE dbo.Tile SET IsMatched = 1 WHERE TileID = @a OR TileID = @b", conn);
            cmd.Parameters.AddWithValue("@a", t1);
            cmd.Parameters.AddWithValue("@b", t2);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
