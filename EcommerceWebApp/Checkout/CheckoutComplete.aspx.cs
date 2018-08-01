using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EcommerceWebApp.Models;

namespace EcommerceWebApp.Checkout
{
  public partial class CheckoutComplete : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        // Verify user has completed the checkout process.
        if ((string)Session["userCheckoutCompleted"] != "true")
        {
          Session["userCheckoutCompleted"] = string.Empty;
          Response.Redirect("CheckoutError.aspx?" + "Desc=Unvalidated%20Checkout.");
        }
          // Clear shopping cart.
          using (EcommerceWebApp.Logic.ShoppingCartActions usersShoppingCart =
              new EcommerceWebApp.Logic.ShoppingCartActions())
          {
            usersShoppingCart.EmptyCart();
          }

          // Clear order id.
          Session["currentOrderId"] = string.Empty;
        }
        
      }
    }
  }
