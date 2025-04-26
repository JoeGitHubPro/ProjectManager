namespace ProjectManager.Models
{
    public class Consumer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Phone { get; set; }
        public string? Whatsapp { get; set; }
        public string? Facebook { get; set; }
        public string? TikTok { get; set; }
        public string? X { get; set; }
        public string? Instagram { get; set; }
        public string? Site { get; set; }
        public string? Notes { get; set; }

        public string? CommercialRegisterNo { get; set; }

        // Navigation
        public ICollection<ProjectOrder> ProjectOrders { get; set; }
    }
}
