using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoKraft.Domain.Enums;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace ChronoKraft.Tests.Integration;

public class TestAccountSeeder
{
    // TODO: Validate this test
    public static async Task<ApplicationUser> SeedTestAccountAsync(
        UserManager<ApplicationUser> userManager, string localPart, UserRole role, string password)
    {
        var email = $"{localPart}@chronokraft.local";
        var existing = await userManager.FindByEmailAsync(email);

        if(existing != null) return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Role = role
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role.ToString());
        return user;
    }
}
