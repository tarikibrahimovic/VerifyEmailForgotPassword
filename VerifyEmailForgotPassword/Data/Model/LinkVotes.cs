namespace VerifyEmailForgotPassword.Data.Model
{
    public class LinkVotes
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public Links Link { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool Vote { get; set; }
        public int IdSadrzaja { get; set; } 
        public string TipSadrzaja { get; set; }
    }
}
