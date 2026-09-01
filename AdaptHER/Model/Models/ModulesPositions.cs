using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class ModulesPositions
    {
        [Required]
        public int ModuleId { get; set; }
        public Modules Modules { get; set; }

        [Required]
        public int PositionId { get; set; }
        public Roles Positions { get; set; }
    }
}
