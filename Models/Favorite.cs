using System.ComponentModel.DataAnnotations;

namespace EchoPlayAPI.Models
{
    public enum FavoriteEntityType
    {
        Track,
        Artist,
        Album
    }

    public class Favorite
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public FavoriteEntityType EntityType { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string SpotifyId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string EntityName { get; set; } = string.Empty;
        
        // Additional metadata for different entity types
        [MaxLength(500)]
        public string? ArtistName { get; set; } // For tracks and albums
        
        [MaxLength(500)]
        public string? AlbumName { get; set; } // For tracks
        
        public int? Duration { get; set; } // For tracks (in milliseconds)
        
        [MaxLength(1000)]
        public string? ImageUrl { get; set; } // Album art, artist image, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Organization
        public int? CategoryId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual FavoriteCategory? Category { get; set; }
    }

    public class FavoriteCategory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(20)]
        public string Color { get; set; } = "#000000"; // Hex color for UI
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}