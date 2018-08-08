using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EcommerceWebApp.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Configuration;
using Newtonsoft.Json;

namespace EcommerceWebApp.Logic
{
  public class ShoppingCartActions : IDisposable
  {
        public static DocumentClient client;
        public static Uri collectionUri;
        public static string _database;
        public static string _collection;

        internal class Event
        {
            public string CartID { get; set; }
            public string Action { get; set; }
            public string Item { get; set; }
            public double Price { get; set; }
        }
        public string ShoppingCartId { get; set; }

    private ProductContext _db = new ProductContext();

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

      var cartItem = _db.ShoppingCartItems.SingleOrDefault(
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
          Product = _db.Products.SingleOrDefault(
           p => p.ProductID == id),
          Quantity = 1,
          DateCreated = DateTime.Now
        };

        _db.ShoppingCartItems.Add(cartItem);
      }
      else
      {
        // If the item does exist in the cart,                  
        // then add one to the quantity.                 
        cartItem.Quantity++;
      }
      _db.SaveChanges();
      //Generate event to send to Cosmos DB
      GenerateEventMain(GetCartId(),"Added", productName, unitPrice);
    }
    
    public void GenerateEventMain(string cartId, string actionEvent, string productName, double unitPrice)
        {
            ConnectionPolicy connectionPolicy = new ConnectionPolicy();
            connectionPolicy.UserAgentSuffix = " samples-net/3";
            connectionPolicy.ConnectionMode = ConnectionMode.Direct;
            connectionPolicy.ConnectionProtocol = Protocol.Tcp;
            connectionPolicy.PreferredLocations.Add(LocationNames.WestUS);
            connectionPolicy.PreferredLocations.Add(LocationNames.NorthEurope);
            connectionPolicy.PreferredLocations.Add(LocationNames.SoutheastAsia);

            Initialize(ConfigurationManager.AppSettings["database"],
                                                          ConfigurationManager.AppSettings["collection"],
                                                          ConfigurationManager.AppSettings["endpoint"],
                                                          ConfigurationManager.AppSettings["authKey"], connectionPolicy);
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
           client.CreateDocumentAsync(collectionUri, e).Wait();
        }

        /*[Initialize: database, collection, endpoint, authkey, connectionpolicy] initializes the databse given the database name, collection name, 
       endpoint Uri, and unique key specified in App.config by using Azure Document Client library*/
        public static void Initialize(string database, string collection, string endpoint, string authkey, ConnectionPolicy connectionPolicy)
        {
            _database = database;
            _collection = collection;
            client = new DocumentClient(new Uri(endpoint), authkey, connectionPolicy);
            collectionUri = UriFactory.CreateDocumentCollectionUri(database, collection);
        }
        public void Dispose()
    {
      if (_db != null)
      {
        _db.Dispose();
        _db = null;
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

      return _db.ShoppingCartItems.Where(
          c => c.CartId == ShoppingCartId).ToList();
    }

    public decimal GetTotal()
    {
      ShoppingCartId = GetCartId();
      // Multiply product price by quantity of that product to get        
      // the current price for each of those products in the cart.  
      // Sum all product price totals to get the cart total.   
      decimal? total = decimal.Zero;
      total = (decimal?)(from cartItems in _db.ShoppingCartItems
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

    public void UpdateShoppingCartDatabase(String cartId, ShoppingCartUpdates[] CartItemUpdates)
    {
      using (var db = new EcommerceWebApp.Models.ProductContext())
      {
        try
        {
          int CartItemCount = CartItemUpdates.Count();
          List<CartItem> myCart = GetCartItems();
          foreach (var cartItem in myCart)
          {
            // Iterate through all rows within shopping cart list
            for (int i = 0; i < CartItemCount; i++)
            {
              if (cartItem.Product.ProductID == CartItemUpdates[i].ProductId)
              {
                if (CartItemUpdates[i].PurchaseQuantity < 1 || CartItemUpdates[i].RemoveItem == true)
                {
                  RemoveItem(cartId, cartItem.ProductId);
                }
                else
                {
                  int differenceInItems = CartItemUpdates[i].PurchaseQuantity - cartItem.Quantity;
                  if (differenceInItems > 0) {
                           using (ShoppingCartActions usersShoppingCart = new ShoppingCartActions())
                           {
                                for (int k = 0; k < differenceInItems; k++)
                                {
                                     usersShoppingCart.AddToCart(cartItem.ProductId, cartItem.Product.ProductName, Convert.ToDouble(cartItem.Product.UnitPrice));
                                }
                                 
                           }
                  }
                  UpdateItem(cartId, cartItem.ProductId, CartItemUpdates[i].PurchaseQuantity);
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
      using (var _db = new EcommerceWebApp.Models.ProductContext())
      {
        try
        {
          var myItem = (from c in _db.ShoppingCartItems where c.CartId == removeCartID && c.Product.ProductID == removeProductID select c).FirstOrDefault();
          if (myItem != null)
          {
            // Remove Item.
            _db.ShoppingCartItems.Remove(myItem);
            _db.SaveChanges();
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
      using (var _db = new EcommerceWebApp.Models.ProductContext())
      {
        try
        {
          var myItem = (from c in _db.ShoppingCartItems where c.CartId == updateCartID && c.Product.ProductID == updateProductID select c).FirstOrDefault();
          if (myItem != null)
          {
            myItem.Quantity = quantity;
            _db.SaveChanges();
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
      var cartItems = _db.ShoppingCartItems.Where(
          c => c.CartId == ShoppingCartId);
      foreach (var cartItem in cartItems)
      {
        _db.ShoppingCartItems.Remove(cartItem);
      }
      // Save changes.             
      _db.SaveChanges();
    }

    public int GetCount()
    {
      string ShoppingCartId = GetCartId();

      // Get the count of each item in the cart and sum them up          
      int? count = (from cartItems in _db.ShoppingCartItems
                    where cartItems.CartId == ShoppingCartId
                    select (int?)cartItems.Quantity).Sum();
      // Return 0 if all entries are null         
      return count ?? 0;
    }

    public struct ShoppingCartUpdates
    {
      public int ProductId;
      public int PurchaseQuantity;
      public bool RemoveItem;
    }

    public void MigrateCart(string cartId, string userName)
    {
      var shoppingCart = _db.ShoppingCartItems.Where(c => c.CartId == cartId);
      foreach (CartItem item in shoppingCart)
      {
        item.CartId = userName;
      }
      HttpContext.Current.Session[CartSessionKey] = userName;
      _db.SaveChanges();
    }
  }
}