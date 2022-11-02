namespace VerifyEmailForgotPassword.Data.Model
{
    public class User_Favorites
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int FavoriteId { get; set; }
        public Favorites Favorite { get; set; }
    }
}
