using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

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
        public JsonObject? DefaultContent { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<PageOverride> Overrides { get; set; } = new List<PageOverride>();
    }
}
