namespace VerifyEmailForgotPassword.Data.Model
{
    public class Favorites
    {
        public int Id { get; set; }
        public int IdSadrzaja { get; set; }
        public string Tip { get; set; }
        public List<User_Favorites> User_Favorites { get; set; }
    }
}
