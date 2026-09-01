using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Modules
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string OrgName { get; set; }
        [Required]
        [MaxLength(300)]
        public string Name { get; set; }
        [Required]
        public DateTime DateStart { get; set; }
        [Required]
        public DateTime DateEnd { get; set; }
        [Required]
        public int StatusId { get; set; }
        public Statuses Statuses { get; set; }

        public ICollection<ModulesPrograms> ModulesPrograms { get; set; }
        public ICollection<ModulesEvents> ModulesEvents { get; set; }
        public ICollection<ModulesPositions> ModulesPositions { get; set; }
        public ICollection<Developers> Developers { get; set; }
        public ICollection<Agreeds> Agreeds { get; set; }
    }
}
