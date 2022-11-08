namespace VerifyEmailForgotPassword.Data.Model
{
    public class Comment
    {
        public int Id { get; set; }
        public string Komentar { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int IdSadrzaja { get; set; }
        public string TipSadrzaja { get; set; }
        public DateTime Date { get; set; }
    }
}
