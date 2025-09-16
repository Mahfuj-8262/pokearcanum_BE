using Microsoft.AspNetCore.Mvc;
using pokearcanumbe.Models;
using Microsoft.EntityFrameworkCore;
using pokearcanumbe.Data;
using Microsoft.AspNetCore.Authorization;

namespace pokearcanumbe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CardController(AppDbContext context, IConfiguration config) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _config = config;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Card>>> GetCards()
        {
            return await _context.Cards.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();
            return card;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Card>> PostCard(CardPostDto dto)
        {
            var card = new Card
            {
                CardName = dto.CardName,
                Hp = dto.Hp,
                Rarity = dto.Rarity,
                Type = dto.Type,
                Link = dto.Link,
                Description = dto.Description
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCard), new { id = card.Id }, card);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<Card>> PutCard(int id, CardputDto dto)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();

            card.CardName = dto.CardName;
            card.Hp = dto.Hp;
            card.Rarity = dto.Rarity;
            card.Type = dto.Type;
            card.Description = dto.Description;
            card.Link = dto.Link;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Card>> CardDelete(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) return NotFound();

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}