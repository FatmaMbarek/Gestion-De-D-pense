using Microsoft.AspNetCore.Identity;

namespace GestionDesDépenses.Models
{
    public class Role : IdentityRole
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
