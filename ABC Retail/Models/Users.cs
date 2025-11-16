using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ABC_Retail.Models
{
    public class Users : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
    }
}
