using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class Person
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page? Page { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Blurb { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public string? ImageStorageKey { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset UpdatedUtc { get; set; }
    }
}
