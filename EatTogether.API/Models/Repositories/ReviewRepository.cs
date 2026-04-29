using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly EatTogetherDBContext _context;

        public ReviewRepository(EatTogetherDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetByDishIdAsync(int dishId)
        {
            return await _context.Reviews
                .Where(r => r.DishId == dishId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task AddAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }
    }
}
