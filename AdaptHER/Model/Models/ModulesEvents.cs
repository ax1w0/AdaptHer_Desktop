using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class ModulesEvents
    {
        [Required]
        public int ModuleId { get; set; }
        public Modules Modules { get; set; }
        [Required]
        public int EventId { get; set; }
        public Events Events { get; set; }
    }
}
