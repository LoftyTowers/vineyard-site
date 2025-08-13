using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VineyardApi.Domain.Content;

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
        public PageContent? OverrideContent { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public Guid UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
    }
}
