using System;
using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class Image
    {
        [Key]
        public Guid Id { get; set; }

        public string StorageKey { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public string? OriginalFilename { get; set; }
        public string? ContentType { get; set; }
        public long? ByteSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
