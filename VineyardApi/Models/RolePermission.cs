using System.ComponentModel.DataAnnotations.Schema;

namespace VineyardApi.Models
{
    public class RolePermission
    {
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        [ForeignKey(nameof(Permission))]
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
