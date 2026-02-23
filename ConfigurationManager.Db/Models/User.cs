using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Db.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();
}