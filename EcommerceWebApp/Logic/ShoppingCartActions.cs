namespace EcommerceWebApp.Logic
{
    using System;
    using EcommerceWebApp.Models;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web;

    public class ShoppingCartActions : IDisposable
    {
        public static DocumentClient Client;
        public static Uri CollectionUri;
        public static string Database;
        public static string Collection;

        static ShoppingCartActions()
        {
            ConnectionPolicy connectionPolicy = new ConnectionPolicy();
            connectionPolicy.UserAgentSuffix = " samples-net/3";
            connectionPolicy.ConnectionMode = ConnectionMode.Direct;
            connectionPolicy.ConnectionProtocol = Protocol.Tcp;
            connectionPolicy.PreferredLocations.Add(LocationNames.WestUS);
            connectionPolicy.PreferredLocations.Add(LocationNames.NorthEurope);
            connectionPolicy.PreferredLocations.Add(LocationNames.SoutheastAsia);

            Database = ConfigurationManager.AppSettings["database"];
            Collection = ConfigurationManager.AppSettings["collection"];
            Client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"], connectionPolicy);
            CollectionUri = UriFactory.CreateDocumentCollectionUri(Database, Collection);
        }

        internal class Event
        {
            public string CartID { get; set; }

            public string Action { get; set; }

            public string Item { get; set; }

            public double Price { get; set; }
        }

        public string ShoppingCartId { get; set; }

        public struct ShoppingCartUpdates
        {
            public int ProductId;
            public int PurchaseQuantity;
            public bool RemoveItem;
        }

        private ProductContext db = new ProductContext();

        public const string CartSessionKey = "CartId";

        public void ViewProduct(int id, string productName, double unitPrice)
        {
            GenerateEventMain(GetCartId(), "Viewed", productName, unitPrice);
        }

        public void PurchaseProduct(int id, string productName, double unitPrice)
        {
            GenerateEventMain(GetCartId(), "Purchased", productName, unitPrice);
        }

        public void AddToCart(int id, string productName, double unitPrice)
        {
            ShoppingCartId = GetCartId();

            var cartItem = db.ShoppingCartItems.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.ProductId == id);
            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists.                 
                cartItem = new CartItem
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ProductId = id,
                    CartId = ShoppingCartId,
                    Product = db.Products.SingleOrDefault(
                   p => p.ProductID == id),
                    Quantity = 1,
                    DateCreated = DateTime.Now
                };

                db.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart,                  
                // then add one to the quantity.                 
                cartItem.Quantity++;
            }

            db.SaveChanges();
            //Generate event to send to Cosmos DB

            GenerateEventMain(GetCartId(), "Added", productName, unitPrice);
        }

        public void GenerateEventMain(string cartId, string actionEvent, string productName, double unitPrice)
        {
            InsertData(GenerateEventHelper(cartId, actionEvent, productName, unitPrice));
        }

        private Event GenerateEventHelper(string cartId, string actionEvent, string productName, double unitPrice)
        {
            Event e = new Event()
            {
                CartID = cartId,
                Action = actionEvent,
                Item = productName,
                Price = unitPrice
            };
            return e;
        }

        /*[InsertData: e] inserts each event e to the database ] by using Azure Document Client library */
        private static void InsertData(Event e)
        {
            Client.CreateDocumentAsync(CollectionUri, e).Wait();
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        public string GetCartId()
        {
            if (HttpContext.Current.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                {
                    HttpContext.Current.Session[CartSessionKey] = HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class.     
                    Guid tempCartId = Guid.NewGuid();
                    HttpContext.Current.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return HttpContext.Current.Session[CartSessionKey].ToString();
        }

        public List<CartItem> GetCartItems()
        {
            ShoppingCartId = GetCartId();

            return db.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).ToList();
        }

        public decimal GetTotal()
        {
            ShoppingCartId = GetCartId();
            // Multiply product price by quantity of that product to get        
            // the current price for each of those products in the cart.  
            // Sum all product price totals to get the cart total.

            decimal? total = decimal.Zero;
            total = (decimal?)(from cartItems in db.ShoppingCartItems
                               where cartItems.CartId == ShoppingCartId
                               select (int?)cartItems.Quantity *
                               cartItems.Product.UnitPrice).Sum();
            return total ?? decimal.Zero;
        }

        public ShoppingCartActions GetCart(HttpContext context)
        {
            using (var cart = new ShoppingCartActions())
            {
                cart.ShoppingCartId = cart.GetCartId();
                return cart;
            }
        }

        public void UpdateShoppingCartDatabase(string cartId, ShoppingCartUpdates[] cartItemUpdates)
        {
            using (var db = new EcommerceWebApp.Models.ProductContext())
            {
                try
                {
                    int cartItemCount = cartItemUpdates.Count();
                    List<CartItem> myCart = GetCartItems();
                    foreach (var cartItem in myCart)
                    {
                        // Iterate through all rows within shopping cart list
                        for (int i = 0; i < cartItemCount; i++)
                        {
                            if (cartItem.Product.ProductID == cartItemUpdates[i].ProductId)
                            {
                                if (cartItemUpdates[i].PurchaseQuantity < 1 || cartItemUpdates[i].RemoveItem == true)
                                {
                                    RemoveItem(cartId, cartItem.ProductId);
                                }
                                else
                                {
                                    int differenceInItems = cartItemUpdates[i].PurchaseQuantity - cartItem.Quantity;
                                    if (differenceInItems > 0)
                                    {
                                        using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
                                        {
                                            for (int k = 0; k < differenceInItems; k++)
                                            {
                                                usersShoppingCart.AddToCart(cartItem.ProductId, cartItem.Product.ProductName, Convert.ToDouble(cartItem.Product.UnitPrice));
                                            }

                                        }
                                    }
                                    UpdateItem(cartId, cartItem.ProductId, cartItemUpdates[i].PurchaseQuantity);
                                }
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("ERROR: Unable to Update Cart Database - " + exp.Message.ToString(), exp);
                }
            }
        }

        public void RemoveItem(string removeCartID, int removeProductID)
        {
            using (ProductContext db = new EcommerceWebApp.Models.ProductContext())
            {
                try
                {
                    var myItem = (from c in db.ShoppingCartItems where c.CartId == removeCartID && c.Product.ProductID == removeProductID select c).FirstOrDefault();
                    if (myItem != null)
                    {
                        // Remove Item.
                        db.ShoppingCartItems.Remove(myItem);
                        db.SaveChanges();
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("ERROR: Unable to Remove Cart Item - " + exp.Message.ToString(), exp);
                }
            }
        }

        public void UpdateItem(string updateCartID, int updateProductID, int quantity)
        {
            using (ProductContext db = new EcommerceWebApp.Models.ProductContext())
            {
                try
                {
                    var myItem = (from c in db.ShoppingCartItems where c.CartId == updateCartID && c.Product.ProductID == updateProductID select c).FirstOrDefault();
                    if (myItem != null)
                    {
                        myItem.Quantity = quantity;
                        db.SaveChanges();
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("ERROR: Unable to Update Cart Item - " + exp.Message.ToString(), exp);
                }
            }
        }

        public void EmptyCart()
        {
            ShoppingCartId = GetCartId();
            var cartItems = db.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId);
            foreach (var cartItem in cartItems)
            {
                db.ShoppingCartItems.Remove(cartItem);
            }

            // Save changes.
            db.SaveChanges();
        }

        public int GetCount()
        {
            string shoppingCartID = GetCartId();

            // Get the count of each item in the cart and sum them up          
            int? count = (from cartItems in db.ShoppingCartItems
                          where cartItems.CartId == shoppingCartID
                          select (int?)cartItems.Quantity).Sum();
            // Return 0 if all entries are null 

            return count ?? 0;
        }

        public void MigrateCart(string cartId, string userName)
        {
            var shoppingCart = db.ShoppingCartItems.Where(c => c.CartId == cartId);
            foreach (CartItem item in shoppingCart)
            {
                item.CartId = userName;
            }

            HttpContext.Current.Session[CartSessionKey] = userName;
            db.SaveChanges();
        }
    }
}