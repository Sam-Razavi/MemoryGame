using Microsoft.Data.SqlClient;
using MemoryGame.Data;
using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public interface ICardRepository
    {
        Task<int> CountAsync();
        Task SeedDefaultPairsAsync();
        Task<List<Card>> GetAllAsync();
    }

    public class CardRepository : ICardRepository
    {
        private readonly IDbConnectionFactory _factory;
        public CardRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<int> CountAsync()
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Card", conn);
            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task<List<Card>> GetAllAsync()
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT CardID, Name, ImagePath, PairKey FROM dbo.Card", conn);
            using var rd = await cmd.ExecuteReaderAsync();
            var list = new List<Card>();
            while (await rd.ReadAsync())
            {
                list.Add(new Card
                {
                    CardID = rd.GetInt32(0),
                    Name = rd.GetString(1),
                    ImagePath = rd.IsDBNull(2) ? null : rd.GetString(2),
                    PairKey = rd.GetString(3)
                });
            }
            return list;
        }

        public async Task SeedDefaultPairsAsync()
        {
            // 8 pairs (16 tiles total) – names are simple; you can swap to images later.
            string[] names = { "Apple", "Car", "Sun", "Moon", "Star", "Tree", "Book", "Fish" };

            using var conn = _factory.Create();
            await conn.OpenAsync();

            foreach (var n in names)
            {
                var exists = new SqlCommand("SELECT 1 FROM dbo.Card WHERE PairKey=@p", conn);
                exists.Parameters.AddWithValue("@p", n);
                var has = await exists.ExecuteScalarAsync();
                if (has is null)
                {
                    var ins = new SqlCommand(
                        "INSERT INTO dbo.Card (Name, ImagePath, PairKey) VALUES (@n, NULL, @p)", conn);
                    ins.Parameters.AddWithValue("@n", n);
                    ins.Parameters.AddWithValue("@p", n);
                    await ins.ExecuteNonQueryAsync();

                    // insert the second member of the pair (same PairKey, different row)
                    var ins2 = new SqlCommand(
                        "INSERT INTO dbo.Card (Name, ImagePath, PairKey) VALUES (@n2, NULL, @p2)", conn);
                    ins2.Parameters.AddWithValue("@n2", n);
                    ins2.Parameters.AddWithValue("@p2", n);
                    await ins2.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
