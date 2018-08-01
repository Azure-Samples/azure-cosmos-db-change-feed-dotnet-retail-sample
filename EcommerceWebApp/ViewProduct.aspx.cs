using Elmah.ContentSyndication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EcommerceWebApp.Logic;

namespace EcommerceWebApp
{
    public partial class ViewProduct : System.Web.UI.Page
    {
        /**When user selects item from product catalog, send product id, product name, and unit price
         * to shopping cart action file to create 'viewed' event in Cosmos DB**/
        protected void Page_Load(object sender, EventArgs e)
        {
            string[] products =  { "Unisex Socks", "Women's Earring", "Women's Necklace" , "Unisex Beanie", "Men's's Baseball Hat", "Unisex Gloves", "Women's Flip Flop Shoes", "Women's Silver Necklace",
            "Men's Black Tee","Men's Black Hoodie","Women's Blue Sweater","Women's Sweatpants","Men's Athletic Shorts","Women's Athletic Shorts","Women's White Sweater","Women's Green Sweater",
            "Men's Windbreaker Jacket","Women's Sandal","Women's Rainjacket","Women's Denim Shorts","Men's Fleece Jacket", "Women's Denim Jacket","Men's's Walking Shoes","Women's Crewneck Sweater",
            "Men's Button-Up Shirt","Women's Flannel Shirt","Women's Light Jeans","Men's Jeans","Women's Dark Jeans", "Women's Red Top","Men's White Shirt","Women's Pant","Women's Blazer Jacket",
            "Men's Puffy Jacket","Women's Puffy Jacket", "Women's Athletic Shoes","Men's Athletic Shoes","Women's Black Dress","Men's Suit Jacket","Men's Suit Pant","Women's High Heel Shoe",
            "Women's Cardigan Sweater", "Men's Dress Shoes","Unisex Puffy Jacket","Women's Red Dress","Unisex Scarf","Women's White Dress","Unisex Sandals","Women's Bag"};
            //string rawId = Request.QueryString["ProductID"];
            int productId;
            string productName = Request.QueryString["ProductName"];
            int id = 0;
            for (int i = 0; i < products.Length; i++)
            {
                if (products[i] == productName)
                {
                    id = i;
                }
            }
            string rawId = id.ToString();
            string unitPrice = Request.QueryString["UnitPrice"];
            if (!String.IsNullOrEmpty(rawId) && int.TryParse(rawId, out productId))
            {
                using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
                {
                    usersShoppingCart.ViewProduct(Convert.ToInt16(rawId), productName, Convert.ToDouble(unitPrice));
                }

            }
            else
            {
                Debug.Fail("ERROR : We should never get to ViewProduct.aspx without a ProductId.");
                throw new Exception("ERROR : It is illegal to load ViewProduct.aspx without setting a ProductId.");
            }
            /**Redirect user to product details page**/
            Response.Redirect(GetRouteUrl("ProductByNameRoute", new { ProductName = productName }));
        }
    }
}
