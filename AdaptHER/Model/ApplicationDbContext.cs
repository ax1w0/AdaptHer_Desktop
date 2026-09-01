using AdaptHER.Class;
using AdaptHER.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace AdaptHER.Model
{
    public sealed class ApplicationDbContext : DbContext
    {
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<Modules> Modules { get; set; }
        public DbSet<Persons> Persons { get; set; }
        public DbSet<Programs> Programs { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Statuses> Statuses { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<LoginAttemptLog> LoginAttemptLogs { get; set; }

        public DbSet<ModulesEvents> ModulesEvents { get; set; }
        public DbSet<ModulesPrograms> ModulesPrograms { get; set; }
        public DbSet<ModulesPositions> ModulesPositions { get; set; }
        public DbSet<Developers> Developers { get; set; }
        public DbSet<RolesDepartments> RolesDepartments { get; set; }
        public DbSet<Agreeds> Agreeds { get; set; }
        public DbSet<Analytics> Analytics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=SERVER;Database=AdaptHER;user id=USER;password=PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Agreeds
            modelBuilder.Entity<Agreeds>()
                .HasKey(nameof(Models.Agreeds.ModuleId), nameof(Models.Agreeds.AgreedId));

            modelBuilder.Entity<Agreeds>()
               .HasOne<Modules>(x => x.Modules)
               .WithMany(x => x.Agreeds)
               .HasForeignKey(x => x.ModuleId)
               .HasConstraintName("FK_Agreed_Modules");

            modelBuilder.Entity<Agreeds>()
                .HasOne<Persons>(x => x.PersonsAgreeds)
                .WithMany(x => x.Agreeds)
                .HasForeignKey(x => x.AgreedId)
                .HasConstraintName("FK_Agreed_Person");
            #endregion

            #region Analytics
            modelBuilder.Entity<Analytics>()
                .HasOne<Programs>(x => x.Programs)
                .WithMany()
                .HasForeignKey(x => x.ProgramId)
                .HasConstraintName("FK_Analytics_Rrogram");

            modelBuilder.Entity<Analytics>()
                .HasOne<Departments>(x => x.Departments)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .HasConstraintName("FK_Analytics_Department");

            modelBuilder.Entity<Analytics>()
               .HasOne<Roles>(x => x.Role)
               .WithMany()
               .HasForeignKey(x => x.RoleId)
               .HasConstraintName("FK_Analytics_Role");
            #endregion

            #region Developers
            modelBuilder.Entity<Developers>()
                .HasKey(nameof(Models.Developers.ModuleId), nameof(Models.Developers.DevelopId));

            modelBuilder.Entity<Developers>()
               .HasOne<Modules>(x => x.Modules)
               .WithMany(x => x.Developers)
               .HasForeignKey(x => x.ModuleId)
               .HasConstraintName("FK_Developers_Modules");

            modelBuilder.Entity<Developers>()
                .HasOne<Persons>(x => x.Develops)
                .WithMany(x => x.Developers)
                .HasForeignKey(x => x.DevelopId)
                .HasConstraintName("FK_Developers_Person");
            #endregion

            #region RolesDepartments
            modelBuilder.Entity<RolesDepartments>()
                .HasKey(nameof(Models.RolesDepartments.RoleId), nameof(Models.RolesDepartments.DepartmentId));

            modelBuilder.Entity<RolesDepartments>()
                .HasOne<Departments>(x => x.Departments)
                .WithMany(x => x.RolesDepartments)
                .HasForeignKey(x => x.DepartmentId)
                .HasConstraintName("FK_EmployeeDepartment_Department");

            modelBuilder.Entity<RolesDepartments>()
               .HasOne<Roles>(x => x.Roles)
               .WithMany(x => x.RolesDepartments)
               .HasForeignKey(x => x.RoleId)
               .HasConstraintName("FK_EmployeeDepartment_Role");
            #endregion

            #region ModulesPositions
            modelBuilder.Entity<ModulesPositions>()
                .HasKey(nameof(Models.ModulesPositions.ModuleId), nameof(Models.ModulesPositions.PositionId));

            modelBuilder.Entity<ModulesPositions>()
                .HasOne<Modules>(x => x.Modules)
                .WithMany(x => x.ModulesPositions)
                .HasForeignKey(x => x.ModuleId)
                .HasConstraintName("FK_ModulePositions_Modules");

            modelBuilder.Entity<ModulesPositions>()
               .HasOne<Roles>(x => x.Positions)
               .WithMany(x => x.ModulesPositions)
               .HasForeignKey(x => x.PositionId)
               .HasConstraintName("FK_ModulePositions_Role");
            #endregion

            #region Modules
            modelBuilder.Entity<Modules>()
                .HasOne<Statuses>(x => x.Statuses)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .HasConstraintName("FK_Modules_Status");
            #endregion

            #region ModulesEvents
            modelBuilder.Entity<ModulesEvents>()
                .HasKey(nameof(Models.ModulesEvents.ModuleId), nameof(Models.ModulesEvents.EventId));

            modelBuilder.Entity<ModulesEvents>()
               .HasOne<Events>(x => x.Events)
               .WithMany(x => x.ModulesEvents)
               .HasForeignKey(x => x.EventId)
               .HasConstraintName("FK_ModulesEvents_Event");

            modelBuilder.Entity<ModulesEvents>()
                .HasOne<Modules>(x => x.Modules)
                .WithMany(x => x.ModulesEvents)
                .HasForeignKey(x => x.ModuleId)
                .HasConstraintName("FK_ModulesEvents_Modules");
            #endregion

            #region ModulesPrograms
            modelBuilder.Entity<ModulesPrograms>()
                .HasKey(nameof(Models.ModulesPrograms.ModuleId), nameof(Models.ModulesPrograms.ProgramId));

            modelBuilder.Entity<ModulesPrograms>()
               .HasOne<Modules>(x => x.Modules)
               .WithMany(x => x.ModulesPrograms)
               .HasForeignKey(x => x.ModuleId)
               .HasConstraintName("FK_ModulesPrograms_Modules");

            modelBuilder.Entity<ModulesPrograms>()
                .HasOne<Programs>(x => x.Programs)
                .WithMany(x => x.ModulesPrograms)
                .HasForeignKey(x => x.ProgramId)
                .HasConstraintName("FK_ModulesPrograms_Program");
            #endregion

            #region Persons
            modelBuilder.Entity<Persons>()
                .HasOne<Roles>(x => x.Roles)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .HasConstraintName("FK_Person_Role");

            modelBuilder.Entity<Persons>()
                .HasOne<Users>(x => x.User)
                .WithMany(u => u.Persons)
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_Person_User");
            #endregion
        }
    }
}