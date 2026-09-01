using System;
using System.ComponentModel.DataAnnotations;

namespace AdaptHER.Model.Models
{
    public sealed class Analytics
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int ProgramId { get; set; }
        public Programs Programs { get; set; }
        [Required]
        public int PersonId { get; set; }
        public Persons Persons { get; set; }
        [Required]
        public int DepartmentId { get; set; }
        public Departments Departments { get; set; }
        [Required]
        public int RoleId { get; set; }
        public Roles Role { get; set; }

        [Required]
        public DateTime DateStart { get; set; }
        [Required]
        public DateTime DateEnd { get; set; }
        [Required]
        public int CountExercise { get; set; }
        [Required]
        public int CountExeCorrect { get; set; }
        [Required]
        public bool IsWorking { get; set; }
    }
}
