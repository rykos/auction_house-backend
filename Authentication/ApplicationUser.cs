using Microsoft.AspNetCore.Identity;

namespace ah_backend.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public double Balance { get; set; }
    }
}