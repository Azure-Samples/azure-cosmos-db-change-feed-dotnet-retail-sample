using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using EcommerceWebApp.Logic;

namespace EcommerceWebApp
{
  public partial class AddToCart : System.Web.UI.Page
  {
    /**When user clicks "add to cart", send product id, product name, and unit price to shopping cart action file to create 'added' event in Cosmos DB**/
    protected void Page_Load(object sender, EventArgs e)
    {
      string rawId = Request.QueryString["ProductID"];
      int productId;
      string productName = Request.QueryString["ProductName"];
      string unitPrice = Request.QueryString["UnitPrice"];
      if (!String.IsNullOrEmpty(rawId) && int.TryParse(rawId, out productId))
      {
        using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
        {
          usersShoppingCart.AddToCart(Convert.ToInt16(rawId), productName, Convert.ToDouble(unitPrice));
        }

      }
      else
      {
        Debug.Fail("ERROR : We should never get to AddToCart.aspx without a ProductId.");
        throw new Exception("ERROR : It is illegal to load AddToCart.aspx without setting a ProductId.");
      }
      /**Redirect user to view their shopping cart**/
      Response.Redirect("ShoppingCart.aspx");
    }
  }
}