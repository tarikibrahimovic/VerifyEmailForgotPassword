namespace VerifyEmailForgotPassword.Data.Model
{
    public class User_Favorites
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int FavoritesId { get; set; }
        public Favorites Favorites { get; set; }
    }
}
