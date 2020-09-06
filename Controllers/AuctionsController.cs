using Microsoft.AspNetCore.Mvc;
using ah_backend.Models;
using ah_backend.Authentication;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Net;
using System.Security.Claims;

namespace ah_backend.Controllers
{
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        UserManager<ApplicationUser> userManager;
        public AuctionsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("")]
        public Auction[] GetAuctions()
        {
            return this.dbContext.Auctions.ToArray();
        }

        [HttpGet]
        [Route("{id}")]
        public Auction GetAuction(int id)
        {
            return dbContext.Auctions.First(x => x.Id == id);
        }

        [Authorize]
        [HttpGet]
        [Route("myauctions")]
        public Auction[] GetMyAuctions()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userId == default)
            {
                return null;
            }
            return dbContext.Auctions.Where(x => x.CreatorId == userId).ToArray();
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateAuction([FromBody] Auction auction)
        {
            if (auction.Title != default && auction.Description != default && auction.Price != default)
            {
                auction.CreatorId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                Auction result = dbContext.Auctions.Add(auction).Entity;
                dbContext.SaveChanges();
                if (result != default)
                {
                    return Created($"api/auctions/{result.Id}", result);
                }
            }
            return BadRequest();
        }
    }
}