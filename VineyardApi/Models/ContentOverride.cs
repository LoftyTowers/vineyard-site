using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class ContentOverride
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page? Page { get; set; }

        [Required]
        [MaxLength(255)]
        public string BlockKey { get; set; } = string.Empty;

        public string HtmlValue { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        [ForeignKey(nameof(ChangedBy))]
        public Guid ChangedById { get; set; }
        public User? ChangedBy { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
