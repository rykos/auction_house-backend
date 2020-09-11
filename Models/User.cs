namespace ah_backend.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public double Balance { get; set; }

        public static User ApplicationUserToUser(Authentication.ApplicationUser applicationUser)
        {
            return new User()
            {
                Id = applicationUser.Id,
                Username = applicationUser.UserName,
                Email = applicationUser.Email,
                Balance = applicationUser.Balance
            };
        }
    }
}