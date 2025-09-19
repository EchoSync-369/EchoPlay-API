using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EchoPlayAPI.Data;
using EchoPlayAPI.Models;
using EchoPlayAPI.DTOs;
using System.Security.Claims;

namespace EchoPlayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
                return 0;

            var user = _context.Users.FirstOrDefault(u => u.Email == emailClaim);
            return user?.Id ?? 0;
        }

        // GET: api/favorites/summary
        [HttpGet("summary")]
        public async Task<ActionResult<FavoritesSummaryResponse>> GetFavoritesSummary()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("User ID is required");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();

            var categories = await _context.FavoriteCategories
                .Where(c => c.UserId == userId)
                .Include(c => c.Favorites)
                .ToListAsync();

            var summary = new FavoritesSummaryResponse
            {
                TotalFavorites = favorites.Count,
                TracksCount = favorites.Count(f => f.EntityType == FavoriteEntityType.Track),
                ArtistsCount = favorites.Count(f => f.EntityType == FavoriteEntityType.Artist),
                AlbumsCount = favorites.Count(f => f.EntityType == FavoriteEntityType.Album),
                CategoriesCount = categories.Count,
                Categories = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    CreatedAt = c.CreatedAt,
                    FavoritesCount = c.Favorites.Count
                }).ToList()
            };

            return Ok(summary);
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<List<FavoriteResponse>>> GetFavorites(
            [FromQuery] FavoriteEntityType? entityType = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool grouped = false)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("User ID is required");

            var query = _context.Favorites
                .Where(f => f.UserId == userId);

            if (entityType.HasValue)
                query = query.Where(f => f.EntityType == entityType.Value);

            if (categoryId.HasValue)
                query = query.Where(f => f.CategoryId == categoryId.Value);

            var favorites = await query
                .Include(f => f.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var favoriteResponses = favorites.Select(f => new FavoriteResponse
            {
                Id = f.Id,
                EntityType = f.EntityType,
                SpotifyId = f.SpotifyId,
                EntityName = f.EntityName,
                ArtistName = f.ArtistName,
                AlbumName = f.AlbumName,
                Duration = f.Duration,
                ImageUrl = f.ImageUrl,
                CreatedAt = f.CreatedAt,
                Category = f.Category != null ? new CategoryResponse
                {
                    Id = f.Category.Id,
                    Name = f.Category.Name,
                    Description = f.Category.Description,
                    Color = f.Category.Color,
                    CreatedAt = f.Category.CreatedAt,
                    FavoritesCount = 0 // Will be populated if needed
                } : null
            }).ToList();

            return Ok(favoriteResponses);
        }

        // GET: api/favorites/grouped
        [HttpGet("grouped")]
        public async Task<ActionResult<List<FavoritesGroupedResponse>>> GetFavoritesGrouped()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var grouped = favorites
                .GroupBy(f => f.Category)
                .Select(g => new FavoritesGroupedResponse
                {
                    Category = g.Key != null ? new CategoryResponse
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Description = g.Key.Description,
                        Color = g.Key.Color,
                        CreatedAt = g.Key.CreatedAt,
                        FavoritesCount = g.Count()
                    } : null,
                    Favorites = g.Select(f => new FavoriteResponse
                    {
                        Id = f.Id,
                        EntityType = f.EntityType,
                        SpotifyId = f.SpotifyId,
                        EntityName = f.EntityName,
                        ArtistName = f.ArtistName,
                        AlbumName = f.AlbumName,
                        Duration = f.Duration,
                        ImageUrl = f.ImageUrl,
                        CreatedAt = f.CreatedAt
                    }).ToList()
                })
                .OrderBy(g => g.Category?.Name ?? "Uncategorized")
                .ToList();

            return Ok(grouped);
        }

        // POST: api/favorites
        [HttpPost]
        public async Task<ActionResult<FavoriteResponse>> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            // Check if category exists (if provided)
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _context.FavoriteCategories
                    .AnyAsync(c => c.Id == request.CategoryId.Value && c.UserId == userId);
                if (!categoryExists)
                    return BadRequest("Category not found or doesn't belong to user");
            }

            // Check for duplicate
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId 
                                       && f.EntityType == request.EntityType 
                                       && f.SpotifyId == request.SpotifyId);

            if (existingFavorite != null)
                return Conflict("This item is already in your favorites");

            var favorite = new Favorite
            {
                UserId = userId,
                EntityType = request.EntityType,
                SpotifyId = request.SpotifyId,
                EntityName = request.EntityName,
                ArtistName = request.ArtistName,
                AlbumName = request.AlbumName,
                Duration = request.Duration,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            // Load the category for response
            if (favorite.CategoryId.HasValue)
            {
                await _context.Entry(favorite)
                    .Reference(f => f.Category)
                    .LoadAsync();
            }

            var response = new FavoriteResponse
            {
                Id = favorite.Id,
                EntityType = favorite.EntityType,
                SpotifyId = favorite.SpotifyId,
                EntityName = favorite.EntityName,
                ArtistName = favorite.ArtistName,
                AlbumName = favorite.AlbumName,
                Duration = favorite.Duration,
                ImageUrl = favorite.ImageUrl,
                CreatedAt = favorite.CreatedAt,
                Category = favorite.Category != null ? new CategoryResponse
                {
                    Id = favorite.Category.Id,
                    Name = favorite.Category.Name,
                    Description = favorite.Category.Description,
                    Color = favorite.Category.Color,
                    CreatedAt = favorite.Category.CreatedAt,
                    FavoritesCount = 0
                } : null
            };

            return CreatedAtAction(nameof(GetFavorites), null, response);
        }

        // DELETE: api/favorites/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (favorite == null)
                return NotFound("Favorite not found");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/favorites/{id}/move
        [HttpPut("{id}/move")]
        public async Task<IActionResult> MoveFavorite(int id, [FromBody] MoveFavoriteRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (favorite == null)
                return NotFound("Favorite not found");

            // Check if category exists (if provided)
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _context.FavoriteCategories
                    .AnyAsync(c => c.Id == request.CategoryId.Value && c.UserId == userId);
                if (!categoryExists)
                    return BadRequest("Category not found or doesn't belong to user");
            }

            favorite.CategoryId = request.CategoryId;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}