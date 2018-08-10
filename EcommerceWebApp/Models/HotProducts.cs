namespace EcommerceWebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    public class HotProduct
    {
        [Key]
        [Required, StringLength(100), Display(Name = "Name")]
        public string Item { get; set; }

        public int CountEvents { get; set; }

        [Display(Name = "Price")]
        public int Price { get; set; }

        public override bool Equals(object obj)
        {
            HotProduct hotItem = obj as HotProduct;
            return (this.Item == hotItem.Item && this.Price == hotItem.Price);
        }

        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }
    }
}