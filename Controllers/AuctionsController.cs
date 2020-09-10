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
        [HttpPost]
        public IActionResult CreateAuction([FromForm] Auction auction, [FromForm] IFormFile icon)
        {
            if (auction != default && auction.Title != default && auction.Description != default && auction.Price != default)
            {
                auction.CreatorId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (icon != null)
                {
                    if (icon.Length < 16000000)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            icon.CopyTo(memoryStream);
                            //auction.Icon = memoryStream.ToArray();
                            string iconFileName = $"images/{auction.CreatorId}_{Guid.NewGuid().ToString()}_{icon.FileName}";
                            auction.Icon = iconFileName;
                            using (FileStream fs = new FileStream(Path.Combine("wwwroot", iconFileName), FileMode.Create))
                            {
                                fs.Write(memoryStream.ToArray());
                            }
                        }
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

        [Authorize]
        [HttpPost]
        [Route("mockAuction/{amount}")]
        public async Task<List<object>> CreateRandomAuction(int amount)
        {
            if (amount < 1)
            {
                return null;
            }
            Random rnd = new Random();
            List<Auction> newAuctions = new List<Auction>();
            string[] titles = { "Buty", "Rekawice", "Naszyjnik", "Pas", "Peleryna" };

            string[] filePaths = Directory.GetFiles(@"C:\develop\IMAGES\MOCK_RESOURCES");
            string creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            for (int i = 0; i < amount; i++)
            {
                string filePath = filePaths[rnd.Next(0, filePaths.Length)];
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        Auction auction = new Auction()
                        {
                            CreatorId = creatorId,
                            Title = titles[rnd.Next(0, titles.Length)],
                            Description = titles[rnd.Next(0, titles.Length)],
                            Price = rnd.Next(10, 1000) + (rnd.Next(0, 101) / 100d),
                        };
                        string iconFileName = $"images/{auction.CreatorId}_{Guid.NewGuid().ToString()}_{Path.GetFileName(filePath)}";
                        using (FileStream fsc = new FileStream(Path.Combine("wwwroot", iconFileName), FileMode.Create))
                        {
                            fsc.Write(ms.ToArray());
                        }
                        auction.Icon = iconFileName;
                        newAuctions.Add(auction);
                        await dbContext.AddAsync(auction);
                    }
                }
            }
            await dbContext.SaveChangesAsync();
            return newAuctions.Select(x => new
            {
                id = x.Id,
                title = x.Title,
                description = x.Description,
                price = x.Price
            }).Cast<object>().ToList();
        }
    }
}