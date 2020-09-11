using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ah_backend.Models
{
    public class Image
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [MaxLength(8)]
        public string Id { get; set; }
        public byte[] Img { get; set; }

        public int AuctionId { get; set; }
    }
}