namespace ProjectManager.Models
{
    public class ProjectCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Navigation
        public ICollection<Project> Projects { get; set; }
    }

}
