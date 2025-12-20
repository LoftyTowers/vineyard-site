using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VineyardApi.Domain.Content;

namespace VineyardApi.Models
{
    public class Page
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Route { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public PageContent DefaultContent { get; set; } = new();

        [ForeignKey(nameof(CurrentVersion))]
        public Guid CurrentVersionId { get; set; }
        public PageVersion? CurrentVersion { get; set; }

        [ForeignKey(nameof(DraftVersion))]
        public Guid? DraftVersionId { get; set; }
        public PageVersion? DraftVersion { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<PageOverride> Overrides { get; set; } = new List<PageOverride>();
        public ICollection<PageVersion> Versions { get; set; } = new List<PageVersion>();
    }
}
