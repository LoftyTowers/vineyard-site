using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class ThemeDefault
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public ICollection<ThemeOverride> Overrides { get; set; } = new List<ThemeOverride>();
    }
}
