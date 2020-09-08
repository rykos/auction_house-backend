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
using System;

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
        [Route("{skip}/{amount}")]
        public Auction[] GetAuctions(int skip, int amount)
        {
            return this.dbContext.Auctions?.Skip(skip).Take(amount)?.ToArray();
        }

        [HttpGet]
        [Route("{id}")]
        public Auction GetAuction(int id)
        {
            return dbContext.Auctions.FirstOrDefault(x => x.Id == id);
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
            return dbContext.Auctions.Where(x => x.CreatorId == userId)?.ToArray();
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

        [Authorize]
        [HttpPost]
        [Route("mockAuction/{amount}")]
        public List<Auction> CreateRandomAuction(int amount)
        {
            if (amount < 1)
            {
                return null;
            }
            Random rnd = new Random();
            List<Auction> newAuctions = new List<Auction>();
            for (int i = 0; i < amount; i++)
            {
                string[] titles = { "Buty", "Rekawice", "Naszyjnik", "Pas", "Peleryna" };
                Auction auction = new Auction()
                {
                    CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Title = titles[rnd.Next(0, titles.Length)],
                    Description = titles[rnd.Next(0, titles.Length)],
                    Price = rnd.Next(10, 1000) + (rnd.Next(0, 101) / 100d)
                };
                newAuctions.Add(auction);
                dbContext.AddAsync(auction);
            }
            dbContext.SaveChanges();
            return newAuctions;
        }
    }
}