using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaptHER.Model.Models
{
    public sealed class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Login { get; set; }

        [Required]
        [MaxLength(255)] 
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(128)]
        public string Salt { get; set; }

        [MaxLength(50)]
        public string Pepper { get; set; } 

        public DateTime? LastLoginTime { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEndTime { get; set; }


        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Persons> Persons { get; set; }
    }
}
