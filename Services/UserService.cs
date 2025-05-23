using GestionDesDépenses.DTOs;
using GestionDesDépenses.Models;
using GestionDesDépenses.Repositories.Interfaces;
using GestionDesDépenses.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestionDesDépenses.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task EnsureUserExistsAsync(string email, string firstName, string lastName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName ?? "Unknown",
                    LastName = lastName ?? "User",
                    DateCreation = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception($"Erreur lors de la création de l'utilisateur : {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task ChangePasswordAsync(string email, ChangePasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Utilisateur non trouvé");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Erreur lors du changement de mot de passe : {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task ForgotPasswordAsync(ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("Utilisateur non trouvé avec cet email");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"https://localhost:5086/api/user/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";

            var message = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <p>Bonjour <strong>{user.FirstName} {user.LastName}</strong>,</p>
                    <p>Vous avez demandé à réinitialiser votre mot de passe.</p>
                    <p><a href='{resetUrl}' style='color: #007bff; text-decoration: none;'>Cliquez ici pour réinitialiser votre mot de passe</a>.</p>
                    <p>Si vous n'avez pas fait cette demande, veuillez ignorer cet email.</p>
                    <p>Merci,</p>
                </div>";
            var subject = "Réinitialisation de votre mot de passe";

            await _emailService.SendEmailAsync(user.Email, subject, message);
        }

        public async Task ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("Utilisateur non trouvé avec cet email");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Erreur lors de la réinitialisation du mot de passe : {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
