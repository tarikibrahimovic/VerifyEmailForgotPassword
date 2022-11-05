using System.ComponentModel.DataAnnotations;

namespace VerifyEmailForgotPassword.Data.Model
{
    public class ChangePassword
    {
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters!")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]//proverava da li je isti confirmpassword i password
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
