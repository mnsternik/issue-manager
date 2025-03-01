using IssueManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
                .HasOne(r => r.AssignedUser)
                .WithMany()
                .HasForeignKey(r => r.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja do zespołu (AssignedToTeam)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.AssignedTeam)
                .WithMany()
                .HasForeignKey(r => r.AssignedTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja do odpowiedzi do złoszenia (Responses)
            modelBuilder.Entity<Request>()
                .HasMany(r => r.Responses)
                .WithOne(rr => rr.Request)
                .HasForeignKey(rr => rr.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Realcja odpowiedzi zgłoszenia do jej autora 
            modelBuilder.Entity<RequestResponse>()
                .HasOne(rr => rr.Author)
                .WithMany()
                .HasForeignKey(rr => rr.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>().HasData(new Team
            {
                Id = 2,
                Name = "Helpdesk"
            });

            modelBuilder.Entity<Category>().HasData(new Category
            {
                Id = 2,
                Name = "Applications"
            });
        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestResponse> RequestResponses { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

    }
}
