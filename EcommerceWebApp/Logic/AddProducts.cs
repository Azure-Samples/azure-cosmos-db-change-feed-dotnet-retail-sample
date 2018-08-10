namespace EcommerceWebApp.Logic
{
    using System;
    using EcommerceWebApp.Models;

    public class AddProducts
    {
        public bool AddProduct(
            string productName,
            string productPrice,
            string productcategory)
        {
            Product myProduct = new Product();
            myProduct.ProductName = productName;
            myProduct.UnitPrice = Convert.ToDouble(productPrice);
            myProduct.CategoryID = Convert.ToInt32(productcategory);

            using (ProductContext db = new ProductContext())
            {
                // Add product to DB.
                db.Products.Add(myProduct);
                db.SaveChanges();
            }

            // Success.
            return true;
        }
    }
}