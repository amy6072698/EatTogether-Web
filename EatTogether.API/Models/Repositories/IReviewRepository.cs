using EatTogether.API.Models.EfModels;

namespace EatTogether.Models.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetByDishIdAsync(int dishId);
        Task AddAsync(Review review);
    }
}
