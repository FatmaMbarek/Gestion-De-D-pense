using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDesDépenses.Models
{
    public class UserPasswordHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateChanged { get; set; }
        public User User { get; set; }
    }
}
