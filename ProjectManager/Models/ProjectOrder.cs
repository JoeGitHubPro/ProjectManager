using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Models;
public class ProjectOrder
{
    public int Id { get; set; }

    public string FolderSerial { get; set; }

    [Range(0, 100)]
    public decimal Discount { get; set; }

    [Range(0, 100)]
    public decimal Commission { get; set; }

    public decimal TotalPrice { get; set; }

    public string? InstallationSystemInfo { get; set; }

    public DateTime SoldDate { get; set; }

    public string? Notes { get; set; }

    // Foreign Key to Consumer.Id
    public int ConsumerId { get; set; }
    public Consumer Consumer { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }
}
