using VerifyEmailForgotPassword.Data.Model;

namespace VerifyEmailForgotPassword.Data.ViewModel
{
    public class FavoritesVM
    {
        public int IdSadrzaja { get; set; }
        public string Tip { get; set; }
        public List<User_Favorites> User_Favorites { get; set; }
    }
}
