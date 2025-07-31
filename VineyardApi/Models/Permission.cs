using System.ComponentModel.DataAnnotations;

namespace VineyardApi.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<RolePermission> Roles { get; set; } = new List<RolePermission>();
    }
}
