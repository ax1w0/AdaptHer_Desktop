using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<RolesDepartments> RolesDepartments { get; set; }
        public ICollection<ModulesPositions> ModulesPositions { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
