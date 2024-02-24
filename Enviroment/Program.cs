using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Enviroment.Services; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add DbContext to the services.
builder.Services.AddDbContext<HelpdeskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HelpdeskContext")));

// Configure Identity services
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HelpdeskContext>();

// Configure EmailService and related services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>(); // Register EmailService for dependency injection
builder.Services.AddScoped<EmailCheckerService>(); // Register EmailCheckerService
builder.Services.AddHostedService<EmailBackgroundService>(); // Register EmailBackgroundService


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

SeedRoles(app.Services).Wait();
SeedUsersAndRoles(app.Services).Wait();
app.Run();

async Task SeedRoles(IServiceProvider serviceProvider)
{
  
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "User", "Admin" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

async Task SeedUsersAndRoles(IServiceProvider serviceProvider)
{
    // Seed users and assign roles
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "User", "Admin" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    await SeedUserAsync(userManager, "Admin@helpdesk.com", "Password1!", "Admin");
    await SeedUserAsync(userManager, "User@gmail.com", "Password1!", "User");
}

async Task SeedUserAsync(UserManager<User> userManager, string email, string password, string role)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new User { UserName = email, Email = email };
        var createUserResult = await userManager.CreateAsync(user, password);
        if (createUserResult.Succeeded)
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, role);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Error adding user {email} to role {role}");
            }

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmEmailResult = await userManager.ConfirmEmailAsync(user, code);
            if (!confirmEmailResult.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user {email}");
            }
        }
        else
        {
            throw new InvalidOperationException($"Error creating user {email}");
        }
    }
}

public class EmailSettings
{
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string? Email { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? Password { get; set; }
    public string? ImapServer { get; set; }
    public int ImapPort { get; set; }
}
