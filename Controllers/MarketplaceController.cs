using Microsoft.AspNetCore.Mvc;
using pokearcanumbe.Models;
using Microsoft.EntityFrameworkCore;
using pokearcanumbe.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using pokearcanumbe.Services;

namespace pokearcanumbe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MarketplaceController(AppDbContext context, IConfiguration config) : ControllerBase
    {
        AppDbContext _context = context;
        IConfiguration _config = config;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marketplace>>> GetMarketplaces()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (userId == null) return Unauthorized();

            return await _context.Marketplaces.Include(m => m.Card).Where(m => m.UserId == userId).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Marketplace>> GetMarketplace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace == null) return NotFound();
            if (marketplace.UserId != userId) return Forbid();

            return marketplace;
        }

        [HttpPost]
        public async Task<ActionResult<Marketplace>> PostMarketplace([FromForm] MarketplacePostDto dto, IFormFile? imageFile, [FromServices] BlobService blobService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);

            string imageUrl = string.Empty;
            if (imageFile != null)
            {
                imageUrl = await blobService.UploadFileAsync(imageFile);
            }

            var card = new Card
            {
                CardName = dto.CardName,
                Hp = dto.Hp,
                Rarity = dto.Rarity,
                Type = dto.Type,
                Link = imageUrl,
                Description = dto.Description
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            var marketplace = new Marketplace
            {
                UserId = userId,
                CardId = card.Id,
                Price = dto.Price,
                Status = ListingStatus.Available,
                Card = card,
                User = user
            };

            _context.Marketplaces.Add(marketplace);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMarketplace), new { id = marketplace.Id }, marketplace);

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Marketplace>> PutMarketplace(int id, [FromBody] MarketplacePutDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace == null) return NotFound();

            if (marketplace.UserId != userId) return Forbid();

            marketplace.Price = dto.Price;
            marketplace.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(marketplace);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Marketplace>> DeleteMarketplace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace == null) return NotFound();

            if (marketplace.UserId != userId) return Forbid();

            _context.Marketplaces.Remove(marketplace);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Marketplace>>> GetAllMarketplaces()
        {
            return await _context.Marketplaces
                .Include(m => m.Card)
                .Include(m => m.User)
                .Where(m => m.Status == ListingStatus.Available)
                .ToListAsync();
        }

        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Marketplace>>> GetTopCards()
        {
            var topCards = await _context.Marketplaces
                .Include(m => m.Card)
                .Where(m => m.Status == ListingStatus.Available)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            return Ok(topCards);
        }

        // [HttpPost("upload")]
        // public async Task<IActionResult> UploadImage(IFormFile file, [FromServices] IConfiguration config)
        // {
        //     if (file == null || file.Length == 0)
        //         return BadRequest("No file uploaded.");

        //     var containerName = config["AzureStorage:ContainerName"];
        //     var connectionString = config["AzureStorage:ConnectionString"];

        //     var blobServiceClient = new BlobServiceClient(connectionString);
        //     var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        //     await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        //     // Give unique name
        //     var blobName = $"{Guid.NewGuid()}-{file.FileName}";
        //     var blobClient = containerClient.GetBlobClient(blobName);

        //     using (var stream = file.OpenReadStream())
        //     {
        //         await blobClient.UploadAsync(stream, true);
        //     }

        //     var blobUrl = blobClient.Uri.ToString();
        //     return Ok(new { url = blobUrl });
        // }

    }
}