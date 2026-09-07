using ChronoKraft.Infrastructure;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* Add services to the container.
***********************************/
// Add DB service
builder.Services.AddDbContext<CKDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ChronoKraftDb")));
// Add App User service
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CKDbContext>()
    .AddDefaultTokenProviders();
// default web interface services
builder.Services.AddRazorPages();

var app = builder.Build();
// Ajouter les 4 roles par defaut au demarrage
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
