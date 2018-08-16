namespace EcommerceWebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Architects every cart on the website
    /// </summary>
    public class CartItem
    {
        [Key]
        public string ItemId { get; set; }

        public string CartId { get; set; }

        public int Quantity { get; set; }

        public System.DateTime DateCreated { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}