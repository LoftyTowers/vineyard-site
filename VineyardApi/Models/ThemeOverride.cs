using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class ThemeOverride
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(ThemeDefault))]
        public int ThemeDefaultId { get; set; }
        public ThemeDefault? ThemeDefault { get; set; }

        public string Value { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public Guid UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
    }
}
