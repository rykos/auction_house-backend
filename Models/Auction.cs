using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ah_backend.Models
{
    public class Auction
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string CreatorId { get; set; }

        [Column(TypeName = "mediumblob")]
        public byte[] Icon { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationTime { get; set; } = DateTime.Now;
    }
}