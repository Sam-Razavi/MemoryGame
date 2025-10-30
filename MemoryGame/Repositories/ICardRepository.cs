using MemoryGame.Models;

namespace MemoryGame.Repositories
{
    public interface ICardRepository
    {
        // Existing helpers you already used
        Task<int> CountAsync();
        Task SeedDefaultPairsAsync();

        // CRUD used by CardAdminController
        Task<List<Card>> GetAllAsync();
        Task<Card?> GetByIdAsync(int id);
        Task CreateAsync(Card card);
        Task UpdateAsync(Card card);
        Task DeleteAsync(int id);
    }
}
