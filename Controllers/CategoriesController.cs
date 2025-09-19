using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EchoPlayAPI.Data;
using EchoPlayAPI.Models;
using EchoPlayAPI.DTOs;

namespace EchoPlayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetCategories([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            var categories = await _context.FavoriteCategories
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Favorites)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                CreatedAt = c.CreatedAt,
                FavoritesCount = c.Favorites.Count
            }).ToList();

            return Ok(response);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(int id, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            var category = await _context.FavoriteCategories
                .Where(c => c.Id == id && c.UserId == user.Id)
                .Include(c => c.Favorites)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Category not found");

            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                FavoritesCount = category.Favorites.Count
            };

            return Ok(response);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> CreateCategory([FromQuery] string email, [FromBody] CreateCategoryRequest request)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            // Check if category name already exists for this user
            var existingCategory = await _context.FavoriteCategories
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.Name == request.Name);

            if (existingCategory != null)
                return Conflict("A category with this name already exists");

            var category = new FavoriteCategory
            {
                UserId = user.Id,
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                CreatedAt = DateTime.UtcNow
            };

            _context.FavoriteCategories.Add(category);
            await _context.SaveChangesAsync();

            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                FavoritesCount = 0
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id, email = user.Email }, response);
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromQuery] string email, [FromBody] UpdateCategoryRequest request)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            var category = await _context.FavoriteCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (category == null)
                return NotFound("Category not found");

            // Check if new name conflicts with existing categories (excluding current one)
            var nameConflict = await _context.FavoriteCategories
                .AnyAsync(c => c.UserId == user.Id && c.Name == request.Name && c.Id != id);

            if (nameConflict)
                return Conflict("A category with this name already exists");

            category.Name = request.Name;
            category.Description = request.Description;
            category.Color = request.Color;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] string email, [FromQuery] bool moveFavoritesToUncategorized = true)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            var category = await _context.FavoriteCategories
                .Where(c => c.Id == id && c.UserId == user.Id)
                .Include(c => c.Favorites)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Category not found");

            if (moveFavoritesToUncategorized)
            {
                // Move all favorites in this category to uncategorized (null)
                foreach (var favorite in category.Favorites)
                {
                    favorite.CategoryId = null;
                }
            }
            else
            {
                // Delete all favorites in this category
                _context.Favorites.RemoveRange(category.Favorites);
            }

            _context.FavoriteCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}