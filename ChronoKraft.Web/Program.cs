using ChronoKraft.Domain.Enums;
using ChronoKraft.Infrastructure;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

/* Add services to the container.
***********************************/
// Add DB service
builder.Services.AddDbContext<CKDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ChronoKraftDb"),
        npgsqlOptions => npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
            dataSourceBuilder.EnableDynamicJson())));
// Add App User service
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CKDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserAccountService>();
// default web interface services
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSerilogRequestLogging(); // Log all HTTP requests automatically

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

/* THIS PART CREATES AN ADMIN ACCOUNT, THAT CAN CREATE OTHER ADMIN ACCOUNTS */
if(args.Contains("--create-admin"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CKDbContext>();
    var userAccountService = scope.ServiceProvider.GetRequiredService<UserAccountService>();

    // Option create-admin must only be used once
    bool adminExists = await db.Users.AnyAsync(u => u.Role == UserRole.ADMIN);
    if(adminExists)
    {
        Log.Warning("An admin account already exists. The --create-admin option can only be used once.");
        Console.WriteLine("An admin account already exists. The --create-admin option can only be used once.");
        return;
    }

    Console.Write("Email de l'administrateur : ");
    var email = Console.ReadLine() ?? throw new InvalidOperationException("Email requis");
    Console.Write("Mot de passe : ");
    var password = ReadPasswordMasked();
    Console.Write("Confirmer le mot de passe : ");
    var confirmPassword = ReadPasswordMasked();
    if (password != confirmPassword)
    {
        Log.Warning("Les mots de passe ne correspondent pas.");
        Console.WriteLine("Les mots de passe ne correspondent pas.");
        return;
    }
    var (result, _) = await userAccountService.CreateUserAccountAsync(email, password, UserRole.ADMIN);

    if (result.Succeeded)
    {
        Log.Information("Compte Admin créé : {Email} par {User}@{Machine} le {DateTime}", email, Environment.UserName, Environment.MachineName, DateTime.UtcNow);
        Console.WriteLine($"Compte Admin créé : {email}");
        Console.WriteLine($"Par {Environment.UserName}@{Environment.MachineName} le {DateTime.UtcNow:u}");
    }
    else
    {
        foreach (var error in result.Errors)
        {
            Log.Error("Erreur lors de la création du compte Admin : {Error}", error.Description);
            Console.WriteLine($"Erreur : {error.Description}");
        }
    }

    return; // quitte avant de démarrer Kestrel
}

app.Run();


static string ReadPasswordMasked()
{
    var password = new System.Text.StringBuilder();
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
            Console.Write("\b \b");
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    } while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password.ToString();
}