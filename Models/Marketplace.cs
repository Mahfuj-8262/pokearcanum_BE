using Microsoft.EntityFrameworkCore;

namespace pokearcanumbe.Models
{
    public class Marketplace
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int CardId { get; set; }
        [Precision(12,4)]
        public decimal Price { get; set; }
        public ListingStatus Status { get; set; } = ListingStatus.Available; //available reserved sold
        public Card? Card { get; set; }
        public User? User { get; set; }
    }

    public class MarketplacePostDto
    {
        public required string CardName { get; set; }
        public int Hp { get; set; }
        public required string Rarity { get; set; }
        public required string Type { get; set; }
        //public required string Link { get; set; }
        public string Description { get; set; } = string.Empty;


        public decimal Price { get; set; }
        public ListingStatus Status { get; set; }
    }

    public class MarketplacePutDto
    {
        public decimal Price { get; set; }
        public ListingStatus Status { get; set; }
    }
}