using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class ImageUsage
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Image))]
        public Guid ImageId { get; set; }
        public Image? Image { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string EntityKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UsageType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? JsonPath { get; set; }

        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty;

        public DateTime UpdatedUtc { get; set; }
    }
}
