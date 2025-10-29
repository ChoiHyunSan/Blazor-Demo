namespace BlazorDashboard.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Mail
    {
        public int Id { get; set; }
        public string title { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public int itemId { get; set; }
        public int itemCount { get; set; }
    }
}
