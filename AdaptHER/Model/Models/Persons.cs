using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Persons
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(30)]
        public string Patronymic { get; set; }
        [Required]
        public DateTime Birthday { get; set; }

        public int? UserId { get; set; }
        public Users User { get; set; }
        public int? RoleId { get; set; }
        public Roles Roles { get; set; }

        public ICollection<Agreeds> Agreeds { get; set; }
        public ICollection<Developers> Developers { get; set; }

        public override string ToString()
        {
            return $"{LastName} {FirstName} {Patronymic}";
        }
    }
}
