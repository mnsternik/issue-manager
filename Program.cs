using IssueManager.Data;
using IssueManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IssueManager.Mapping;
using IssueManager.Utilities;

namespace IssueManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; 
            })
                .AddRoles<IdentityRole>() 
                .AddEntityFrameworkStores<ApplicationDbContext>(); 

            builder.Services.AddControllersWithViews();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddTransient<DataSeeder>(); 

            var app = builder.Build();

            // Creating roles and admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dataSeeder = services.GetRequiredService<DataSeeder>();

                    await dataSeeder.CreateRoles();
                    await dataSeeder.CreateAdminUser();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating roles or admin user: {ex.Message}, {ex.InnerException}");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); 
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Requests}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }   
    }
}