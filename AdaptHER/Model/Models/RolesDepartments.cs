using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class RolesDepartments
    {
        [Required]
        public int RoleId { get; set; }
        public Roles Roles { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public Departments Departments { get; set; }
    }
}
