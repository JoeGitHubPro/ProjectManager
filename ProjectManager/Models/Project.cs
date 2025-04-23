using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManager.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Details { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Foreign Key
        public int ProjectCategoryId { get; set; }

        // Navigation
        public ProjectCategory ProjectCategory { get; set; }

        public ICollection<ProjectOrder> ProjectOrders { get; set; }
    }

}
