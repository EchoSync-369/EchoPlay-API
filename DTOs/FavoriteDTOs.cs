using System.ComponentModel.DataAnnotations;
using EchoPlayAPI.Models;

namespace EchoPlayAPI.DTOs
{
    // Request DTOs
    public class AddFavoriteRequest
    {
        [Required]
        public FavoriteEntityType EntityType { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string SpotifyId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string EntityName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ArtistName { get; set; }
        
        [MaxLength(500)]
        public string? AlbumName { get; set; }
        
        public int? Duration { get; set; }
        
        [MaxLength(1000)]
        public string? ImageUrl { get; set; }
        
        public int? CategoryId { get; set; }
    }

    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(20)]
        public string Color { get; set; } = "#000000";
    }

    public class UpdateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(20)]
        public string Color { get; set; } = "#000000";
    }

    public class MoveFavoriteRequest
    {
        public int? CategoryId { get; set; }
    }

    // Response DTOs
    public class FavoriteResponse
    {
        public int Id { get; set; }
        public FavoriteEntityType EntityType { get; set; }
        public string SpotifyId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? ArtistName { get; set; }
        public string? AlbumName { get; set; }
        public int? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public CategoryResponse? Category { get; set; }
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int FavoritesCount { get; set; }
    }

    public class FavoritesGroupedResponse
    {
        public CategoryResponse? Category { get; set; }
        public List<FavoriteResponse> Favorites { get; set; } = new();
    }

    public class FavoritesSummaryResponse
    {
        public int TotalFavorites { get; set; }
        public int TracksCount { get; set; }
        public int ArtistsCount { get; set; }
        public int AlbumsCount { get; set; }
        public int CategoriesCount { get; set; }
        public List<CategoryResponse> Categories { get; set; } = new();
    }
}