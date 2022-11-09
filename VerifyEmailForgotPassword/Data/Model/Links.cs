namespace VerifyEmailForgotPassword.Data.Model
{
    public class Links
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int IdSadrzaja { get; set; }
        public string TipSadrzaja { get; set; }
        public string Date { get; set; }
        public List<LinkVotes> Votes { get; set; }
    }
}
