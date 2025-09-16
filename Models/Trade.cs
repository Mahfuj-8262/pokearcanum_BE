using Microsoft.EntityFrameworkCore;

namespace pokearcanumbe.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public required string SellerId { get; set; }
        public required string BuyerId { get; set; }
        public int MarketplaceId { get; set; }
        [Precision(12,4)]
        public decimal Amount { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public required User Seller { get; set; }
        public required User Buyer { get; set; }
        public required Marketplace Marketplace { get; set; }
    }

    public class PostTradeDto
    {
        public int MarketplaceId { get; set; }
    }
}