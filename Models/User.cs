using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace GestionDesDépenses.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public int? IsFirstConnection { get; set; }
        public DateTime? LastConnection { get; set; }
        public DateTime? ResetPassword { get; set; }
        public DateTime DateCreation { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
        public DateTime? DateMajPwd { get; set; }
        public ICollection<UserPasswordHistory> PasswordHistory { get; set; } = new List<UserPasswordHistory>();
    }
}
