using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EcommerceWebApp.Models;

namespace EcommerceWebApp.Logic
{
  public class AddProducts
  {
    public bool AddProduct(
        string ProductName, 
        string ProductPrice, 
        string ProductCategory)
    {
      var myProduct = new Product();
      myProduct.ProductName = ProductName;
      myProduct.UnitPrice = Convert.ToDouble(ProductPrice);
      myProduct.CategoryID = Convert.ToInt32(ProductCategory);

      using (ProductContext _db = new ProductContext())
      {
        // Add product to DB.
        _db.Products.Add(myProduct);
        _db.SaveChanges();
      }

      // Success.
      return true;
    }
  }
}