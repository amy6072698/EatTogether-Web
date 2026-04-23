using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories
{
    public interface IMemberFavoriteRepository
    {
        Task<List<int>> GetFavoriteProductIdsByMemberIdAsync(int memberId);
    }
    public class MemberFavoriteRepository : IMemberFavoriteRepository
    {
        private readonly EatTogetherDBContext _context;
        public MemberFavoriteRepository(EatTogetherDBContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetFavoriteProductIdsByMemberIdAsync(int memberId)
        {
            return await _context.MemberFavorites
                .Where(f => f.MemberId == memberId)
                .Select(f => f.ProductId)
                .ToListAsync();
        }
    }
}
