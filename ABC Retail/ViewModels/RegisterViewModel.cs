using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.ViewModels
{
    public class RegisterViewModel
    {
        [Required (ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required (ErrorMessage ="Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required (ErrorMessage ="Password is required.")]
        [StringLength (40, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} characters long, but less than {1} long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password does not match.")]
        public string ConfirmPassword { get; set; }
    }
}
