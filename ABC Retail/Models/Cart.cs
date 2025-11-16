using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABC_Retail.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; }

        public string? ProductRowKey { get; set; }  
        public string? ProductPartitionKey { get; set; }

        public string? ProductName { get; set; }
        public double? ProductPrice { get; set; }

        public int Quantity { get; set; }
    }
}
