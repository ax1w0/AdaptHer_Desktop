using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Developers
    {
        [Required]
        public int ModuleId { get; set; }
        public Modules Modules { get; set; }

        [Required]
        public int DevelopId { get; set; }
        public Persons Develops { get; set; }
    }
}
