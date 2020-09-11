using Microsoft.AspNetCore.Mvc;
using ah_backend.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using ah_backend.Models;

namespace ah_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("details")]
        public async Task<IActionResult> GetAccountDetails()
        {
            ApplicationUser user = await this.userManager.GetUserAsync(User);
            if (user == default)
            {
                return NotFound();
            }
            return Ok(Models.User.ApplicationUserToUser(user));
        }

        [HttpPost]
        [Route("Removeaccount")]
        public async Task<IActionResult> RemoveAccount()
        {
            ApplicationUser user = await this.userManager.GetUserAsync(User);
            if (user == default)
            {
                return BadRequest();
            }
            await this.userManager.DeleteAsync(user);
            return Ok();
        }

        [HttpPost]
        [Route("balance/add/{amount}")]
        public async Task<IActionResult> AddBalance(double amount)
        {
            if (amount == default || amount < 0)
                return BadRequest(new Response { Status = "Error", Message = "Invalid amount" });
            ApplicationUser user = await this.userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == default)
                return BadRequest(new Response { Status = "Error", Message = "User not found" });

            dbContext.Update(user);
            user.Balance += amount;
            dbContext.SaveChanges();

            return Ok(Models.User.ApplicationUserToUser(user));
        }

        
    }
}