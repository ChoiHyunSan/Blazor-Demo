namespace BlazorDashboard.Models
{
    public class ServerInfo
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = ""; // Online / Offline
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }
}
