using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database connection
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity configuration
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            // === SESSION CONFIGURATION ===
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // === DATABASE INITIALIZATION & SEEDING ===
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Applying migrations...");
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();

                    logger.LogInformation("Seeding roles...");
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    string[] roles = { "HR", "Lecturer", "Coordinator", "Manager" };
                    foreach (var roleName in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                            logger.LogInformation($"Created role: {roleName}");
                        }
                    }

                    logger.LogInformation("Seeding users...");
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    string password = "Test123!";

                    await CreateUser(userManager, "hr@example.com", password, "HR", logger);
                    await CreateUser(userManager, "lecturer@example.com", password, "Lecturer", logger);
                    await CreateUser(userManager, "coordinator@example.com", password, "Coordinator", logger);
                    await CreateUser(userManager, "manager@example.com", password, "Manager", logger);

                    logger.LogInformation("Database setup completed successfully!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database setup failed");
                }
            }

            // === PIPELINE CONFIGURATION ===
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

            app.UseSession(); // <-- MUST be after routing but before endpoints

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        private static async Task CreateUser(UserManager<ApplicationUser> userManager, string email, string password, string role, ILogger logger)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = role,
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation($"Created user: {email} with role: {role}");
                }
                else
                {
                    logger.LogError($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User exists: {email}");
            }
        }
    }
}
