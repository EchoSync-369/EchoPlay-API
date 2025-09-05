namespace EchoPlayAPI.Models
{
    public class SpotifyUser
    {
        public string id { get; set; } = string.Empty;
        public string display_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public List<SpotifyImage> images { get; set; } = new();
        public string country { get; set; } = string.Empty;
    }

    public class SpotifyImage
    {
        public string url { get; set; } = string.Empty;
        public int height { get; set; }
        public int width { get; set; }
    }
}