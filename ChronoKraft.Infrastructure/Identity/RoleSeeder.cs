using ChronoKraft.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ChronoKraft.Infrastructure.Identity
{
    public static class RoleSeeder
    {
        /// <summary>
        /// Seeds all roles defined by the <see cref="UserRole"/> enumeration.
        /// </summary>
        /// <param name="roleManager">The manager used to check and create roles.</param>
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in Enum.GetValues<UserRole>())
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }
    }
}