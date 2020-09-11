using Microsoft.AspNetCore.Mvc;
using ah_backend.Authentication;
using ah_backend.Models;
using System.Linq;

namespace ah_backend.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class Images : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public Images(ApplicationDbContext applicationDbContext)
        {
            this.dbContext = applicationDbContext;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetImage(string id)
        {
            Image img = dbContext.Images.FirstOrDefault(x => x.Id == id);
            if (img == default)
                return NotFound();

            return File(img.Img, "image/jpeg");
        }
    }
}