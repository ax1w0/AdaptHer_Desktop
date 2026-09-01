using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AdaptHER.Model.Models
{
    [Table("LoginAttemptLogs")]
    public class LoginAttemptLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required]
        public bool IsSuccess { get; set; }

        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; }

        [Required]
        public DateTime AttemptTime { get; set; }

        [MaxLength(200)]
        public string FailureReason { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}