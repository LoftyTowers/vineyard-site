using System;
using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class Image
    {
        [Key]
        public Guid Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
