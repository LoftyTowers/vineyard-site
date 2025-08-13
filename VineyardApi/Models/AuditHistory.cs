using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class AuditHistory
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(AuditLog))]
        public Guid AuditLogId { get; set; }
        public AuditLog? AuditLog { get; set; }

        [Column(TypeName = "jsonb")]
        public string? PreviousValue { get; set; }
        [Column(TypeName = "jsonb")]
        public string? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
