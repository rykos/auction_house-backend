using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ah_backend.Models
{
    public class FinishedAuction : Auction
    {
        [MaxLength(100)]
        public string BuyerId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime BuyDateTime { get; set; } = DateTime.Now;
    }
}