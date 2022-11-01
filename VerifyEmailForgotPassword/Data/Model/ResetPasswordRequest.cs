using System.ComponentModel.DataAnnotations;

namespace VerifyEmailForgotPassword.Model
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters!")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]//proverava da li je isti confirmpassword i password
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
