namespace VerifyEmailForgotPassword.Data.ViewModel
{
    public class LinkVM
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public int IdSadrzaja { get; set; }
        public string TipSadrzaja { get; set; }
    }
}
