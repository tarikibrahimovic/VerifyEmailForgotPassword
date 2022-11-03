using VerifyEmailForgotPassword.Data.Model;

namespace VerifyEmailForgotPassword.Data.ViewModel
{
    public class LinkVotesVM
    {
        public int Id { get; set; }
        public bool Vote { get; set; }
        public int IdSadrzaja { get; set; }
        public string TipSadrzaja { get; set; }
    }
}
