using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Models
{
    public class Consumer
    {
        [Key] // Mark Phone as the primary key
        [Phone]
        public string Phone { get; set; }

        public string? CommercialRegisterNo { get; set; }

        // Navigation
        public ICollection<ProjectOrder> ProjectOrders { get; set; }
    }


}
