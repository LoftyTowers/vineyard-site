using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

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
        public JsonObject? PreviousValue { get; set; }
        [Column(TypeName = "jsonb")]
        public JsonObject? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
