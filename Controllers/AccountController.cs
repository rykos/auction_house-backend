using Microsoft.AspNetCore.Mvc;
using ah_backend.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ah_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountDetails()
        {
            ApplicationUser user = await this.userManager.GetUserAsync(User);
            if (user == default)
            {
                return NotFound();
            }
            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                balance = user.Balance
            });
        }

        [HttpPost]
        [Route("Remove")]
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
    }
}