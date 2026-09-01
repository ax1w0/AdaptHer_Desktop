using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Agreeds
    {
        [Required]
        public int ModuleId { get; set; }
        public Modules Modules { get; set; }

        [Required]
        public int AgreedId { get; set; }
        public Persons PersonsAgreeds { get; set; }
    }
}
