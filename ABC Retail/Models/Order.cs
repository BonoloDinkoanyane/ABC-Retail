using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ABC_Retail.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string? UserId { get; set; }
        public Users? User { get; set; }

        [Required]
        public DateTime? OrderDate { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending"; //default status is pending

        public decimal TotalAmount { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
} 