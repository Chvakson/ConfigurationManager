namespace ConfigurationManager.Db.Models
{
    public class Configuration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<ConfigurationVersion> Versions { get; set; } = new List<ConfigurationVersion>();
    }
}
