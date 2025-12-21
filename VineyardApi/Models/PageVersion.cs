using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VineyardApi.Domain.Content;

namespace VineyardApi.Models
{
    public enum PageVersionStatus
    {
        Published = 0,
        Draft = 1,
        Archived = 2
    }

    public class PageVersion
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page? Page { get; set; }

        public int VersionNo { get; set; }

        [Column(TypeName = "jsonb")]
        public PageContent ContentJson { get; set; } = new();

        public PageVersionStatus Status { get; set; } = PageVersionStatus.Published;
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? PublishedUtc { get; set; }

        public DateTime CreatedUtc { get; set; }
        public string? CreatedBy { get; set; }
        public string? ChangeNote { get; set; }
        public string? ContentHash { get; set; }
    }
}
