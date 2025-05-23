using System.ComponentModel.DataAnnotations;

namespace GestionDesDépenses.DTOs
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
