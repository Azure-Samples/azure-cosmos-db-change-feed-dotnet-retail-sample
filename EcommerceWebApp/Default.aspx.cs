using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Security;
using EcommerceWebApp.Models;
using EcommerceWebApp.Logic;
using System.Data.Entity;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Configuration;
using System.Threading.Tasks;

namespace EcommerceWebApp
{
    public partial class _Default : Page
    {
        private static readonly Uri _endpointUri = new Uri(ConfigurationManager.AppSettings["endpoint"]);
        private static readonly string _primaryKey = ConfigurationManager.AppSettings["authKey"];

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private void Page_Error(object sender, EventArgs e)
        {
            // Get last error from the server.
            Exception exc = Server.GetLastError();

            // Handle specific exception.
            if (exc is InvalidOperationException)
            {
                // Pass the error on to the error page.
                Server.Transfer("ErrorPage.aspx?handler=Page_Error%20-%20Default.aspx",
                    true);
            }
        }

        /// <summary>
        /// Returns top best selling items on the website
        /// </summary>
        /// <returns>top best selling items in the database returned by a sql query</returns>
        public IQueryable GetHotItems()
        {
            ProductContext _db = new EcommerceWebApp.Models.ProductContext();
            int hotItemsCount = _db.HotItems.Count<HotProduct>();
            if (hotItemsCount > 0)
            {
                Clear(_db.HotItems);
                _db.SaveChanges();
            }

            UpdateHotItems(_db.HotItems);
            _db.SaveChanges();
            IQueryable query = _db.HotItems;
            return query;
        }

        /// <summary>
        /// Updates the Local DbSet to be in sync with the Cosmos DB collection
        /// </summary>
        /// <param name="db"> a DbSet instance of the DbContext that contains Best Selling Items</param>
        private void UpdateHotItems(DbSet<HotProduct> db)
        {
            DocumentClient Client = new DocumentClient(_endpointUri, _primaryKey);
            Uri collectionSelfLink3 = UriFactory.CreateDocumentCollectionUri(ConfigurationManager.AppSettings["database"], ConfigurationManager.AppSettings["hotItemsCollection"]);

            List<dynamic> topItems = ReadAllDocumentsInCollectionAsync(Client, collectionSelfLink3);
            List<HotProduct> hotItems = new List<HotProduct>();
            foreach (dynamic popular in topItems)
            {
                HotProduct hot = popular;
                if (!hotItems.Contains(hot))
                {
                    hotItems.Add(hot);
                }
            }
            hotItems.ForEach(i=> db.Add(i));
        }

        /// <summary>
        /// Removes all HotProduct entities in a DbSet
        /// </summary>
        /// <param name="db"> A DbSet of hot products</param>
        private void Clear(DbSet<HotProduct> db)
        {
            foreach (HotProduct item in db)
            {
                db.Remove(item);
            }
        }

        /// <summary>
        /// Read all the documents from a Cosmos DB collection
        /// </summary>
        /// <param name="client">The Client to the Cosmos DB account</param>
        /// <param name="collectionSelfLink">A Link to the Cosmos DB collection</param>
        /// <returns>A List of dynamic documents read from Cosmos DB</returns>
        private List<dynamic> ReadAllDocumentsInCollectionAsync(DocumentClient client, Uri collectionSelfLink)
        {
            List<dynamic> documents = new List<dynamic>();

            string continuationToken = null;
            do
            {
                FeedResponse<dynamic> feedResponseOfProducts = client.ReadDocumentFeedAsync(collectionSelfLink, new FeedOptions() { RequestContinuation = continuationToken }).Result;
                documents.AddRange(feedResponseOfProducts);
                continuationToken = feedResponseOfProducts.ResponseContinuation;
            } while (continuationToken != null);

            return documents;
        }

    }
}