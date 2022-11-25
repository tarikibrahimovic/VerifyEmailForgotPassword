using System.Security.Claims;
using VerifyEmailForgotPassword.Data.Model;

namespace VerifyEmailForgotPassword.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PassswordSalt { get; set; } = new byte[32];
        public string? VerificationToken { get; set; }
        public string Role { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public string? PictureUrl { get; set; }
        public List<User_Favorites> User_Favorites { get; set; }
        public List<Comment> Komentar { get; set; }
        public List<Links> Linkovi { get; set; }
        public List<LinkVotes> Votes { get; set; }
    }
}
