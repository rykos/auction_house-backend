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
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ah_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return this.dbContext.Auctions?.OrderByDescending(x => x.CreationTime).Skip(skip).Take(amount)?.ToArray();
        }

        [HttpGet]
        [Route("finished")]
        public Auction[] GetFinishedAuctions()
        {
            return this.dbContext.FinishedAuctions.ToArray();
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
            return dbContext.Auctions.Where(x => x.CreatorId == userId)?.OrderByDescending(x => x.CreationTime).ToArray();
        }

        [Authorize]
        [HttpGet]
        [Route("my/bought")]
        public async Task<IActionResult> GeyMyBoughtAuctions()
        {
            ApplicationUser user = await this.userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == default)
            {
                return Unauthorized();
            }
            Auction[] auctions = this.dbContext.FinishedAuctions.Where(x => x.BuyerId == user.Id).OrderByDescending(x => x.CreationTime).ToArray();
            return Ok(auctions);
        }

        [Authorize]
        [HttpGet]
        [Route("my/sold")]
        public async Task<IActionResult> GeyMySoldAuctions()
        {
            ApplicationUser user = await this.userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == default)
            {
                return Unauthorized();
            }
            Auction[] auctions = this.dbContext.FinishedAuctions.Where(x => x.CreatorId == user.Id).OrderByDescending(x => x.CreationTime).ToArray();
            return Ok(auctions);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromForm] Auction auction, [FromForm] IFormFile icon)
        {
            ApplicationUser user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == default)
            {
                return Unauthorized();
            }

            if (auction != default && auction.Title != default && auction.Description != default && auction.Price != default)
            {
                auction.CreatorId = user.Id;
                if (icon != null)
                {
                    if (icon.Length < 16000000)
                    {
                        this.SaveImageToDb(icon, ref auction);
                    }
                    else
                    {
                        return BadRequest(new Response { Status = "Error", Message = "Icon file size is too large" });
                    }
                }
                Auction result = dbContext.Auctions.Add(auction).Entity;
                dbContext.SaveChanges();
                if (result != default)
                {
                    return Created($"api/auctions/{result.Id}", result);
                }
            }
            return BadRequest(new Response { Status = "Error", Message = "Missing fields" });
        }

        //Only for testing
        [Authorize]
        [HttpPost]
        [Route("mockAuction/{amount}")]
        public async Task<List<object>> CreateMockAuction(int amount)
        {
            if (amount < 1)
            {
                return null;
            }
            Random rnd = new Random();
            List<Auction> newAuctions = new List<Auction>();
            string[] titles = { "Shoes", "Helmet", "Neklece", "Sword", "Staff" };

            string[] filePaths = Directory.GetFiles(@"C:\develop\IMAGES\MOCK_RESOURCES");
            string creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            for (int i = 0; i < amount; i++)
            {
                string filePath = filePaths[rnd.Next(0, filePaths.Length)];
                Auction auction = new Auction()
                {
                    CreatorId = creatorId,
                    Title = titles[rnd.Next(0, titles.Length)],
                    Description = titles[rnd.Next(0, titles.Length)],
                    Price = rnd.Next(10, 1000) + (rnd.Next(0, 101) / 100d),
                };

                using (FileStream fs = System.IO.File.Open(filePath, FileMode.Open))
                {
                    this.SaveImageToDb(fs, ref auction);
                }

                newAuctions.Add(auction);
                dbContext.Add(auction);
            }
            dbContext.SaveChanges();
            return newAuctions.Select(x => new
            {
                id = x.Id,
                title = x.Title,
                description = x.Description,
                price = x.Price
            }).Cast<object>().ToList();
        }

        [HttpPost]
        [Authorize]
        [Route("buy")]
        public async Task<IActionResult> BuyItem([FromBody] TransactionConfirmation transaction)
        {
            ApplicationUser user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == default)
            {
                return Unauthorized();
            }

            Auction auction = dbContext.Auctions.FirstOrDefault(x => x.Id == transaction.AuctionId);
            ApplicationUser recipient = await this.userManager.FindByIdAsync(auction.CreatorId);

            if (auction == default || recipient == default)
            {
                return NotFound();
            }

            if (user.Balance < auction.Price)
            {
                return BadRequest(new { msg = "Balance is too low" });
            }

            lock (dbContext.Auctions)
            {
                dbContext.Auctions.Remove(auction);

                FinishedAuction finishedAuction = new FinishedAuction()
                {
                    Id = auction.Id,
                    Price = auction.Price,
                    BuyerId = user.Id,
                    CreationTime = auction.CreationTime,
                    CreatorId = auction.CreatorId,
                    Description = auction.Description,
                    IconId = auction.IconId,
                    Title = auction.Title
                };
                dbContext.FinishedAuctions.Add(finishedAuction);

                dbContext.Users.UpdateRange(new ApplicationUser[] { user, recipient });
                user.Balance -= auction.Price;
                recipient.Balance += auction.Price;

                dbContext.SaveChanges();
            }

            return Ok();
        }

        private void SaveImageToDb(IFormFile icon, ref Auction auction)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                icon.CopyTo(memoryStream);
                Image img = new Image()
                {
                    AuctionId = auction.Id,
                    Img = memoryStream.ToArray()
                };
                auction.IconId = dbContext.Add(img).Entity.Id;
            }
            dbContext.SaveChanges();
        }
        private void SaveImageToDb(FileStream fs, ref Auction auction)
        {
            IFormFile formFile = new FormFile(fs, 0, fs.Length, "", fs.Name);
            this.SaveImageToDb(formFile, ref auction);
        }
    }
}