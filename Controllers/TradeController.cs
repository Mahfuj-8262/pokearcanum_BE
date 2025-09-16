using Microsoft.AspNetCore.Mvc;
using pokearcanumbe.Models;
using Microsoft.EntityFrameworkCore;
using pokearcanumbe.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace pokearcanumbe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TradeController(AppDbContext context, IConfiguration config) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _config = config;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTrades()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return await _context.Trades
            .Include(t => t.Buyer)
            .Include(t => t.Seller)
            .Include(t => t.Marketplace)
                .ThenInclude(m => m.Card)
            .Where(t => t.BuyerId == userId || t.SellerId == userId)
            .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Trade>> GetTrade(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var trade = await _context.Trades
            .Include(t => t.Buyer)
            .Include(t => t.Seller)
            .Include(t => t.Marketplace)
                .ThenInclude(m => m.Card)
            .FirstOrDefaultAsync(t => t.Id == id);
            if (trade == null) return NotFound();
            if (trade.BuyerId != userId && trade.SellerId != userId) return Forbid();

            return trade;
        }

        [HttpPost]
        public async Task<ActionResult<Trade>> PostTrade(PostTradeDto dto)
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

            var marketplace = await _context.Marketplaces.FirstOrDefaultAsync(m => m.Id == dto.MarketplaceId);
            if (marketplace == null) return NotFound();
            if (marketplace.Status != ListingStatus.Available) return BadRequest("This Card is not available!");
            if (marketplace.UserId == buyerId) return BadRequest("You can not buy your own Card!");

            var buyer = await _context.Users.FindAsync(buyerId);
            if (buyer == null) return Unauthorized("Buyer not found");
            var seller = await _context.Users.FindAsync(marketplace.UserId);
            if (seller == null) return NotFound("Seller does not exist!");

            var trade = new Trade
            {
                SellerId = marketplace.UserId,
                BuyerId = buyerId,
                MarketplaceId = marketplace.Id,
                Amount = marketplace.Price,
                Buyer = buyer,
                Seller = seller,
                Marketplace = marketplace,
            };

            _context.Trades.Add(trade);

            marketplace.Status = ListingStatus.Sold;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrade), new { id = trade.Id }, trade);
        }

        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTradeStats()
        {
            var trades = await _context.Trades
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Marketplace)
                    .ThenInclude(m => m.Card)
                .OrderByDescending(t => t.Time)
                .Take(30) // last 30 trades
                .Select(t => new
                {
                    id = t.Id,
                    amount = t.Amount,
                    time = t.Time,
                    buyer = t.Buyer.UserName,
                    seller = t.Seller.UserName,
                    card = t.Marketplace.Card!.CardName
                })
                .ToListAsync();

            return Ok(trades);
        }

        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentTrades()
        {
            var trades = await _context.Trades
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Marketplace)
                    .ThenInclude(m => m.Card)
                .OrderByDescending(t => t.Time)
                .Take(5) // recent 5
                .Select(t => new
                {
                    id = t.Id,
                    time = t.Time,
                    user = t.Buyer.UserName,   // buyer name
                    partner = t.Seller.UserName,
                    card = t.Marketplace.Card!.CardName
                })
                .ToListAsync();

            return Ok(trades);
        }

    }
}