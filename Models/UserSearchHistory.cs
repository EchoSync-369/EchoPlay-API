using System;

namespace EchoPlayAPI.Models
{
    public class UserSearchHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Query { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}