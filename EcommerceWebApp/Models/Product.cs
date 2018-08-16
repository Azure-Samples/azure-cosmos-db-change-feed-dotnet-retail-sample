namespace EcommerceWebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the architecture of every product object
    /// </summary>
    public class Product
    {
        [ScaffoldColumn(false)]
        public int ProductID { get; set; }

        [Required, StringLength(100), Display(Name = "Name")]
        public string ProductName { get; set; }

        [Display(Name = "Price")]
        public double? UnitPrice { get; set; }

        public int? CategoryID { get; set; }

        public virtual Category Category { get; set; }
    }
}