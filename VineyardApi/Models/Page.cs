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

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<PageOverride> Overrides { get; set; } = new List<PageOverride>();
    }
}
