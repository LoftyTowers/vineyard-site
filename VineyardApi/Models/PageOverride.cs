using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace VineyardApi.Models
{
    public class PageOverride
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page? Page { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonObject? OverrideContent { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public Guid UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
    }
}
