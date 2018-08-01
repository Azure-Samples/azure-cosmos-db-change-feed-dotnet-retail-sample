namespace EcommerceWebApp.Models
{
    using System.Data.Entity;

    public class ProductContext : DbContext
    {
        public ProductContext()
          : base("EcommerceWebApp")
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<CartItem> ShoppingCartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<HotProduct> HotItems { get; set; }
    }
}