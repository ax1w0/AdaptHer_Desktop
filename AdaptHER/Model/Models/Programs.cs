using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Programs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        public ICollection<ModulesPrograms> ModulesPrograms { get; set; }
    }
}
