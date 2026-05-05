using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
{
    public interface IOrderRepository 
    {
        Task AddWithPaymentAsync(Order order, Payment payment);
        Task<List<Order>> GetRecentByMemberIdAsync(int memberId, int take = 5);
    }
    public class OrderRepository : IOrderRepository 
    {
        private readonly EatTogetherDBContext _context;
        public OrderRepository(EatTogetherDBContext db) => _context = db;
        public async Task AddWithPaymentAsync(Order order, Payment payment)
        {
            // 先存 Payment 取得 Id
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 再存 Order，帶入 PaymentId
            order.PaymentId = payment.Id;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 回填 Payment.OrderId
            payment.OrderId = order.Id;
            await _context.SaveChangesAsync();
        }
        public async Task<List<Order>> GetRecentByMemberIdAsync(int memberId, int take = 5)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.PreOrder)
                    .ThenInclude(po => po.PreOrderDetails)  // 備援：OrderDetails 空時改用 PreOrderDetails
                .Where(o => o.MemberId == memberId)
                .OrderByDescending(o => o.OrderAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
