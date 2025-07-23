using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    }
}
