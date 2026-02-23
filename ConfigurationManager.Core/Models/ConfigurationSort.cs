using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Core.Models;

public enum ConfigurationSort
{
    [Display(Name = "DateAsc", Description = "По дате (старые-новые)")]
    DateAsc = 0,

    [Display(Name = "DateDesc", Description = "По дате (новые-старые)")]
    DateDesc = 1,

    [Display(Name = "NameAsc", Description = "По имени (А-Я)")]
    NameAsc = 2,

    [Display(Name = "NameDesc", Description = "По имени (Я-А)")]
    NameDesc = 3
}