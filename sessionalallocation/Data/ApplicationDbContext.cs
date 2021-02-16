using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SessionalAllocation.Models;

namespace SessionalAllocation.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
    ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Applications> Applications { get; set; }
        //public virtual DbSet<ApplicationUser> ApplicationUser { get; set; }
        public virtual DbSet<Faculty> Faculty { get; set; }
        public virtual DbSet<School> School { get; set; }
        public virtual DbSet<Class> Class { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<Payrate> Payrate { get; set; }
        public virtual DbSet<TutorPreference> TutorPreference { get; set; }
        public virtual DbSet<TimeAvailability> TimeAvailability { get; set; }
        public IConfiguration Configuration { get; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            new AspNetUsersConfiguration(modelBuilder.Entity<ApplicationUser>());
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            //Specifying enum to string conversion for Qualifications.
            modelBuilder.Entity<ApplicationUser>().Property(e => e.Qualification).HasConversion<string>();

            //Configuring Applications
            modelBuilder.Entity<Applications>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PK__Applicat__C93A4C99FF7BBAB0");

                entity.Property(e => e.ApplicationId).ValueGeneratedOnAdd();

                entity.Property(e => e.Applicant)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.ApplicantNavigation)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.Applicant)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Applicant_ID_Identity");

                entity.HasOne(d => d.AppliedClassNavigation)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.AppliedClass)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppliedClass_ID");
            });

            //modelBuilder.Entity<ApplicationUser>(entity =>
            //{
            //    entity.HasIndex(e => e.NormalizedEmail)
            //        .HasName("EmailIndex");

            //    entity.HasIndex(e => e.NormalizedUserName)
            //        .HasName("UserNameIndex")
            //        .IsUnique()
            //        .HasFilter("([NormalizedUserName] IS NOT NULL)");

            //    entity.Property(e => e.Id).ValueGeneratedNever();

            //    entity.Property(e => e.Email).HasMaxLength(256);

            //    entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

            //    entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

            //    entity.Property(e => e.UserName).HasMaxLength(256);
            //});

            //Configuring Classes
            modelBuilder.Entity<Class>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ClassType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.TutorAllocated)
                    .HasMaxLength(450);

                entity.HasOne(d => d.TutorAllocatedNavigation)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.TutorAllocated)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tutor_Id");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Unit_ID");
            });

            //Configuring Faculties
            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name).HasMaxLength(450);
            });

            //Configuring Schools
            modelBuilder.Entity<School>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name).HasMaxLength(450);

                entity.HasOne(d => d.FacultyNavigation)
                    .WithMany(p => p.School)
                    .HasForeignKey(d => d.Faculty)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Faculty_Name");
            });

            //Configuring Departments
            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name).HasMaxLength(450);

                entity.HasOne(d => d.SchoolNavigation)
                    .WithMany(p => p.Department)
                    .HasForeignKey(d => d.School)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_School_Name");

                entity.HasOne(d => d.DepartmentOwnerNavigation)
                    .WithMany(p => p.Department)
                    .HasForeignKey(d => d.DepartmentOwner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DepartmentOwner_ID");
            });

            //Configuring Units
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.UnitName)
                    .HasMaxLength(100);

                entity.Property(e => e.UnitOwner)
                    .HasMaxLength(450);

                entity.HasOne(d => d.DepartmentNavigation)
                    .WithMany(p => p.Unit)
                    .HasForeignKey(d => d.Department)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_Name");

                entity.HasOne(d => d.UnitOwnerNavigation)
                    .WithMany(p => p.Unit)
                    .HasForeignKey(d => d.UnitOwner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UnitOwner_ID");
            });

            //Configuring User Roles for Identity/Users
            modelBuilder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            //Configuring Payrates
            modelBuilder.Entity<Payrate>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TutorPreference>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            //Configuring Time Availabilities for Applicants

        }
    }
}
