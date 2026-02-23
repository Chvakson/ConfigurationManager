using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Db.Models
{
    public class ConfigurationVersion
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        [Required]
        public string SettingsJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid ConfigurationId { get; set; }
        public Configuration Configuration { get; set; } = null!;
    }
}
