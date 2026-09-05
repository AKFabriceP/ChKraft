using Microsoft.AspNetCore.Identity;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public required UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}