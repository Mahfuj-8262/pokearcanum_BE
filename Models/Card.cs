namespace pokearcanumbe.Models
{
    public class Card
    {
        public int Id { get; set; }
        public required string CardName { get; set; }
        public int Hp { get; set; }
        public required string Rarity { get; set; }
        public required string Type { get; set; }
        public required string Link { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CardPostDto
    {
        public required string CardName { get; set; }
        public int Hp { get; set; }
        public required string Rarity { get; set; }
        public required string Type { get; set; }
        public required string Link { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    public class CardputDto
    {
        public required string CardName { get; set; }
        public int Hp { get; set; }
        public required string Rarity { get; set; }
        public required string Type { get; set; }
        public required string Link { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}