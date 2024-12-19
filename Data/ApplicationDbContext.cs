using IssueManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace IssueManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacja do użytkownika (AssignedToUser)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.AssignedToUser)
                .WithMany()
                .HasForeignKey(r => r.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Relacja do zespołu (AssignedToTeam)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.AssignedToTeam)
                .WithMany()
                .HasForeignKey(r => r.AssignedToTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>().HasData(new Team
            {
                Id = 1,
                Name = "Default Team"
            });

            modelBuilder.Entity<Category>().HasData(new Category
            {
                Id = 1,
                Name = "Default Category"
            });
        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

    }
}
