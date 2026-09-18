using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChronoKraft.Infrastructure.Identity
{
    public class UserAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CKDbContext _dbContext;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(UserManager<ApplicationUser> userManager, CKDbContext dbContext, ILogger<UserAccountService> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new application user account and assigns the specified role.
        /// </summary>
        /// <param name="email">The email address used as the username and contact email.</param>
        /// <param name="password">The password for the new user account.</param>
        /// <param name="role">The role assigned to the user after creation.</param>
        /// <returns>
        /// A tuple containing the Identity result and the created user, or null if the creation failed.
        /// </returns>
        public async Task<(IdentityResult, ApplicationUser?)> CreateUserAccountAsync(string email, string password, UserRole role)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Role = role
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user account for {Email}. Errors: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return (result, null);
            }
            _logger.LogInformation("Successfully created user account for {Email} with role {Role}.", email, role);

            // Assign the specified role to the user
            var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {Email}. Errors: {Errors}", role, email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return (roleResult, null);
            }
            _logger.LogInformation("Successfully assigned role {Role} to user {Email}.", role, email);
            return (result, user);
        }

        /// <summary>
        /// Links a worker to a user account.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task LinkWorkerAsync(Worker worker, int userId)
        {
            worker.UserId = userId;
            _dbContext.Workers.Update(worker);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Linked worker {WorkerId} to user account {UserId}.", worker.Id, userId);
        }

        /// <summary>
        /// Links a project manager to a user account.
        /// </summary>
        /// <param name="projectManager"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task LinkProjectManagerAsync(ProjectManager projectManager, int userId)
        {
            projectManager.UserId = userId;
            _dbContext.ProjectManagers.Update(projectManager);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Linked project manager {ProjectManagerId} to user account {UserId}.", projectManager.Id, userId);
        }

        /// <summary>
        /// Links a contractor to a user account.
        /// </summary>
        /// <param name="contractor"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task LinkContractorAsync(Contractor contractor, int userId)
        {
            contractor.UserId = userId;
            _dbContext.Contractors.Update(contractor);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Linked contractor {ContractorId} to user account {UserId}.", contractor.Id, userId);
        }

        /// <summary>
        /// Creates a new user account and generates a password reset link for the user.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async 
        Task<(IdentityResult Result, string? ResetToken, ApplicationUser? User)> 
        CreateAccountWithResetLinkAsync(string email, UserRole role)
        {
            // Create user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Role = role
            };
            // No password, user will initialize it via reset link
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user account for {Email}. Errors: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return (result, null, null);
            }
            await _userManager.AddToRoleAsync(user, role.ToString());
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Generated password reset token for user {Email} as {Role}.", email, role.ToString());
            return (result, token, user);
        }

        public async Task LinkExternalLoginAsync(ApplicationUser user, ExternalLoginInfo loginInfo)
        {
            await _userManager.AddLoginAsync(user, loginInfo);
            _logger.LogInformation("Linked external login {LoginProvider} for user {Email}.", loginInfo.LoginProvider, user.Email);

            // Annulation de l option de connection par mot de passe: ceci reste commenté pour le moment
            // await _userManager.RemovePasswordAsync(user);
            // _logger.LogInformation("Removed password login option for user {Email}.", user.Email);
        }
    }
}