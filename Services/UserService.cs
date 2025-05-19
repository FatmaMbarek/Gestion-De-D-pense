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
        private readonly SignInManager<User> _signInManager;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager,
            IUserRepository userRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateCreation = DateTime.UtcNow,
                IsFirstConnection = 1,
               
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new Exception($"Erreur lors de l'inscription : {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(user, "Utilisateur");
        }

        public async Task<string> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return null;

            user.LastConnection = DateTime.UtcNow;
            user.IsFirstConnection = 0;
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false);
            return GenerateJwtToken(user);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task UpdateAsync(string userId, UpdateUserModel model)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("Utilisateur non trouvé");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
        
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Utilisateur non trouvé");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Erreur lors du changement de mot de passe : {string.Join(", ", result.Errors.Select(e => e.Description))}");

            user.DateMajPwd = DateTime.UtcNow;
            user.PasswordHistory.Add(new UserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                DateChanged = DateTime.UtcNow
            });
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task LogoutAllUsersAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user); // Invalide tous les tokens existants
                await _signInManager.SignOutAsync();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Utilisateur")
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
    }
}
