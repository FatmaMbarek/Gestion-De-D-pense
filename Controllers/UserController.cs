using System.Security.Claims;
using GestionDesDépenses.DTOs;
using GestionDesDépenses.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionDesDépenses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "User", new { returnUrl = model.ReturnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            var info = await HttpContext.AuthenticateAsync("Google");
            if (info == null || !info.Succeeded)
            {
                return Unauthorized("Échec de l'authentification externe.");
            }

            var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email non trouvé dans les claims de l'utilisateur.");
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Utilisateur non trouvé.");
            }

            var token = await _userService.GenerateJwtTokenAsync(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            return Login(new LoginModel { ReturnUrl = "/" }); // Redirect to OIDC login
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized("Utilisateur non authentifié.");

                await _userService.ChangePasswordAsync(email, model);
                return Ok("Mot de passe changé avec succès");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            try
            {
                await _userService.ForgotPasswordAsync(model);
                return Ok("Un email de réinitialisation a été envoyé.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                await _userService.ResetPasswordAsync(model);
                return Ok("Mot de passe réinitialisé avec succès.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

