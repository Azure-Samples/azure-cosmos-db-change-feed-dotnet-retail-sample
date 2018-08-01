﻿namespace EcommerceWebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public string Username { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public double? UnitPrice { get; set; }
    }
}