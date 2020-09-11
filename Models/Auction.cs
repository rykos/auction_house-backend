using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ah_backend.Models
{
    public class Auction
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string CreatorId { get; set; }

        public string IconId { get; set; }

        [Required(ErrorMessage = "Tile is required")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double Price { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationTime { get; set; } = DateTime.Now;
    }
}