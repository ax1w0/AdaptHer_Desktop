using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class ModulesPrograms
    {
        [Required]
        public int ModuleId { get; set; }
        public Modules Modules { get; set; }

        [Required]
        public int ProgramId { get; set; }
        public Programs Programs { get; set; }
    }
}
