namespace EchoPlayAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public bool IsAdmin { get; set; }
    }
}