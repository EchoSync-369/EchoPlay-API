using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EchoPlayAPI.Data;
using EchoPlayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EchoPlayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSearchHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserSearchHistoryController(AppDbContext context)
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

        [HttpPost]
        public IActionResult AddSearch([FromBody] SearchQueryDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(dto.Query) || userId == 0)
                return BadRequest();

            // Deduplicate: check if a search with the same query (case-insensitive) already exists for this user
            var existing = _context.UserSearchHistories
                .FirstOrDefault(h => h.UserId == userId && h.Query.ToLower() == dto.Query.Trim().ToLower());

            if (existing != null)
            {
                // Optionally update the timestamp to move it to the top
                existing.UpdatedAt = DateTime.UtcNow;
                existing.CreatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return Ok(existing);
            }

            var entry = new UserSearchHistory
            {
                UserId = userId,
                Query = dto.Query.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSearchHistories.Add(entry);
            _context.SaveChanges();

            return Ok(entry);
        }

        [HttpGet]
        public IActionResult GetRecentSearches()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest();

            var searches = _context.UserSearchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(20)
                .ToList();

            return Ok(searches);
        }

        [HttpGet("all/{userId}")]
        public IActionResult GetUserSearches(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return BadRequest();

            var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (user == null || !user.IsAdmin)
                return Forbid();

            var searches = _context.UserSearchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToList();

            return Ok(searches);
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteSearch(int id)
        {
            var userId = GetCurrentUserId();
            var entry = _context.UserSearchHistories.FirstOrDefault(h => h.Id == id && h.UserId == userId);
            if (entry == null) return NotFound();
            _context.UserSearchHistories.Remove(entry);
            _context.SaveChanges();
            return NoContent();
        }
    }

    public class SearchQueryDto
    {
        public string Query { get; set; } = string.Empty;
    }
}