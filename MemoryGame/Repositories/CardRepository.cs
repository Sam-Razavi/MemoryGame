using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly IDbConnectionFactory _factory;
        public CardRepository(IDbConnectionFactory factory) => _factory = factory;

        // ---------- CRUD ----------

        public async Task<List<Card>> GetAllAsync()
        {
            const string sql = @"SELECT CardID, Name, PairKey FROM dbo.Card ORDER BY CardID;";
            var list = new List<Card>();
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new Card
                {
                    CardID = rd.GetInt32(0),
                    Name = rd.GetString(1),
                    PairKey = rd.GetString(2)
                });
            }
            return list;
        }

        public async Task<Card?> GetByIdAsync(int id)
        {
            const string sql = @"SELECT CardID, Name, PairKey FROM dbo.Card WHERE CardID=@id;";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new Card
            {
                CardID = rd.GetInt32(0),
                Name = rd.GetString(1),
                PairKey = rd.GetString(2)
            };
        }

        public async Task CreateAsync(Card card)
        {
            const string sql = @"INSERT INTO dbo.Card (Name, PairKey) VALUES (@n, @p);";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", card.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("@p", card.PairKey ?? string.Empty);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Card card)
        {
            const string sql = @"UPDATE dbo.Card SET Name=@n, PairKey=@p WHERE CardID=@id;";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", card.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("@p", card.PairKey ?? string.Empty);
            cmd.Parameters.AddWithValue("@id", card.CardID);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM dbo.Card WHERE CardID=@id;";
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ---------- Existing helpers (kept for game seeding) ----------

        public async Task<int> CountAsync()
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Card;", conn);
            var val = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(val ?? 0);
        }

        // Seed a small default set of pairs if table is empty
        public async Task SeedDefaultPairsAsync()
        {
            if (await CountAsync() > 0) return;

            var defaults = new (string Name, string PairKey)[]
            {
                ("Apple",  "FRUIT_A"),
                ("Apple",  "FRUIT_A"),
                ("Banana", "FRUIT_B"),
                ("Banana", "FRUIT_B"),
                ("Cat",    "ANIMAL_C"),
                ("Cat",    "ANIMAL_C"),
                ("Dog",    "ANIMAL_D"),
                ("Dog",    "ANIMAL_D")
            };

            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                foreach (var d in defaults)
                {
                    var cmd = new SqlCommand(
                        "INSERT INTO dbo.Card (Name, PairKey) VALUES (@n, @p);",
                        conn, tran);
                    cmd.Parameters.AddWithValue("@n", d.Name);
                    cmd.Parameters.AddWithValue("@p", d.PairKey);
                    await cmd.ExecuteNonQueryAsync();
                }
                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }
    }
}
