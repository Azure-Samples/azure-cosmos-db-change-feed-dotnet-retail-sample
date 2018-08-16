namespace EcommerceWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Linq;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// Initializes the database of shopping items
    /// </summary>
    public class ProductDatabaseInitializer : DropCreateDatabaseAlways<ProductContext>
    {
        private static readonly Uri EndpointUri = new Uri(ConfigurationManager.AppSettings["endpoint"]);
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["authKey"];
        private static readonly string MainDatabase = ConfigurationManager.AppSettings["database"];
        private static readonly string ProductsCollection = ConfigurationManager.AppSettings["productsCollection"];
        private static readonly string CategoriesCollection = ConfigurationManager.AppSettings["categoriesCollection"];
        private static readonly string HotItemsCollection = ConfigurationManager.AppSettings["hotItemsCollection"];

        /// <summary>
        /// Extracts and adds the categories and Products
        /// </summary>
        /// <param name="context">provides the context using ProductContext.cs (which uses DbContext) for the Categories and Products</param>
        protected override void Seed(ProductContext context)
        {
            Uri collectionSelfLink = UriFactory.CreateDocumentCollectionUri(MainDatabase, ProductsCollection);
            Uri collectionSelfLink2 = UriFactory.CreateDocumentCollectionUri(MainDatabase, CategoriesCollection);
            Uri collectionSelfLink3 = UriFactory.CreateDocumentCollectionUri(MainDatabase, HotItemsCollection);
            DocumentClient client = new DocumentClient(EndpointUri, PrimaryKey);

            List<dynamic> categories = ReadAllDocumentsInCollectionAsync(client, collectionSelfLink2);
            List<dynamic> products = ReadAllDocumentsInCollectionAsync(client, collectionSelfLink);
            if (products.Count() <= 0)
            {
                SeedProductsInCosmosDB(client, collectionSelfLink);
                products = ReadAllDocumentsInCollectionAsync(client, collectionSelfLink);
            }

            if (categories.Count() <= 0)
            {
                SeedCategoriesInDB(client, collectionSelfLink2);
                categories = ReadAllDocumentsInCollectionAsync(client, collectionSelfLink2);
            }

            List<dynamic> popularItems = ReadAllDocumentsInCollectionAsync(client, collectionSelfLink3);
            List<Product> items = new List<Product>();
            List<Category> catalog = new List<Category>();
            List<HotProduct> hotProducts = new List<HotProduct>();

            foreach (dynamic product in products)
            {
                items.Add((Product)product);
            }

            foreach (dynamic category in categories)
            {
                catalog.Add((Category)category);
            }

            foreach (dynamic popularItem in popularItems)
            {
                if (!hotProducts.Contains((HotProduct)popularItem))
                {
                    hotProducts.Add((HotProduct)popularItem);
                }
            }

            items.ForEach(product => context.Products.Add(product));
            catalog.ForEach(category => context.Categories.Add(category));
            hotProducts.ForEach(hotitem => context.HotItems.Add(hotitem));
        }

        /// <summary>
        /// Reads all the documents in a Cosmos DB collection
        /// </summary>
        /// <param name="client"> The client to a Cosmos DB account</param>
        /// <param name="collectionSelfLink"> A Uri to the the collection we are looking at</param>
        /// <returns>A list of type-dynamic documents in a specified collection </returns>
        private static List<dynamic> ReadAllDocumentsInCollectionAsync(DocumentClient client, Uri collectionSelfLink)
        {
            List<dynamic> documents = new List<dynamic>();

            string continuationToken = null;
            do
            {
                FeedResponse<dynamic> feedResponseOfProducts = client.ReadDocumentFeedAsync(collectionSelfLink, new FeedOptions() { RequestContinuation = continuationToken }).Result;
                documents.AddRange(feedResponseOfProducts);
                continuationToken = feedResponseOfProducts.ResponseContinuation;
            }
            while (continuationToken != null);
            return documents;
        }

        /// <summary>
        /// Sends the products catalog to a CosmosDB collection
        /// </summary>
        /// <param name="client"> The client to a Cosmos DB account</param>
        /// <param name="collectionSelfLink"> A Uri to the the collection we are looking at</param>
        private static void SeedProductsInCosmosDB(DocumentClient client, Uri collectionSelfLink)
        {
            List<Product> products = new List<Product>
                {
                    new Product
                    {
                        ProductID = 1,
                        ProductName = "Unisex Socks",
                        UnitPrice = 3.75,
                        CategoryID = 3
                    },
                    new Product
                    {
                        ProductID = 2,
                        ProductName = "Women's Earring",
                        UnitPrice = 8.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 3,
                        ProductName = "Women's Necklace",
                        UnitPrice = 12.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 4,
                        ProductName = "Unisex Beanie",
                        UnitPrice = 10.00,
                        CategoryID = 3
                    },
                    new Product
                    {
                        ProductID = 5,
                        ProductName = "Men's's Baseball Hat",
                        UnitPrice = 17.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 6,
                        ProductName = "Unisex Gloves",
                        UnitPrice = 20.00,
                        CategoryID = 3
                    },
                    new Product
                    {
                        ProductID = 7,
                        ProductName = "Women's Flip Flop Shoes",
                        UnitPrice = 14.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 8,
                        ProductName = "Women's Silver Necklace",
                        UnitPrice = 15.50,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 9,
                        ProductName = "Men's Black Tee",
                        UnitPrice = 9.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 10,
                        ProductName = "Men's Black Hoodie",
                        UnitPrice = 25.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 11,
                        ProductName = "Women's Blue Sweater",
                        UnitPrice = 27.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 12,
                        ProductName = "Women's Sweatpants",
                        UnitPrice = 21.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 13,
                        ProductName = "Men's Athletic Shorts",
                        UnitPrice = 22.50,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 14,
                        ProductName = "Women's Athletic Shorts",
                        UnitPrice = 22.50,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 15,
                        ProductName = "Women's White Sweater",
                        UnitPrice = 32.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 16,
                        ProductName = "Women's Green Sweater",
                        UnitPrice = 30.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 17,
                        ProductName = "Men's Windbreaker Jacket",
                        UnitPrice = 49.99,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 18,
                        ProductName = "Women's Sandal",
                        UnitPrice = 35.50,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 19,
                        ProductName = "Women's Rainjacket",
                        UnitPrice = 55.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 20,
                        ProductName = "Women's Denim Shorts",
                        UnitPrice = 50.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 21,
                        ProductName = "Men's Fleece Jacket",
                        UnitPrice = 65.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 22,
                        ProductName = "Women's Denim Jacket",
                        UnitPrice = 31.99,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 23,
                        ProductName = "Men's's Walking Shoes",
                        UnitPrice = 79.99,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 24,
                        ProductName = "Women's Crewneck Sweater",
                        UnitPrice = 22.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 25,
                        ProductName = "Men's Button-Up Shirt",
                        UnitPrice = 19.99,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 26,
                        ProductName = "Women's Flannel Shirt",
                        UnitPrice = 19.99,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 27,
                        ProductName = "Women's Light Jeans",
                        UnitPrice = 80.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 28,
                        ProductName = "Men's Jeans",
                        UnitPrice = 85.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 29,
                        ProductName = "Women's Dark Jeans",
                        UnitPrice = 90.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 30,
                        ProductName = "Women's Red Top",
                        UnitPrice = 33.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 31,
                        ProductName = "Men's White Shirt",
                        UnitPrice = 25.20,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 32,
                        ProductName = "Women's Pant",
                        UnitPrice = 40.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 33,
                        ProductName = "Women's Blazer Jacket",
                        UnitPrice = 87.50,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 34,
                        ProductName = "Men's Puffy Jacket",
                        UnitPrice = 99.99,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 35,
                        ProductName = "Women's Puffy Jacket",
                        UnitPrice = 95.99,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 36,
                        ProductName = "Women's Athletic Shoes",
                        UnitPrice = 75.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 37,
                        ProductName = "Men's Athletic Shoes",
                        UnitPrice = 70.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 38,
                        ProductName = "Women's Black Dress",
                        UnitPrice = 65.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 39,
                        ProductName = "Men's Suit Jacket",
                        UnitPrice = 92.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 40,
                        ProductName = "Men's Suit Pant",
                        UnitPrice = 95.00,
                        CategoryID = 2
                    },
                    new Product
                    {
                        ProductID = 41,
                        ProductName = "Women's High Heel Shoe",
                        UnitPrice = 72.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 42,
                        ProductName = "Women's Cardigan Sweater",
                        UnitPrice = 25.00,
                        CategoryID = 1
                    },
                    new Product
                    {
                        ProductID = 43,
                        ProductName = "Men's Dress Shoes",
                        UnitPrice = 120.00,
                        CategoryID = 2
                    },
                        new Product
                    {
                        ProductID = 44,
                        ProductName = "Unisex Puffy Jacket",
                        UnitPrice = 105.00,
                        CategoryID = 3
                    },
                        new Product
                    {
                        ProductID = 45,
                        ProductName = "Women's Red Dress",
                        UnitPrice = 130.00,
                        CategoryID = 1
                    },
                        new Product
                    {
                        ProductID = 46,
                        ProductName = "Unisex Scarf",
                        UnitPrice = 29.99,
                        CategoryID = 3
                    },
                        new Product
                    {
                        ProductID = 47,
                        ProductName = "Women's White Dress",
                        UnitPrice = 84.99,
                        CategoryID = 1
                    },
                        new Product
                    {
                        ProductID = 48,
                        ProductName = "Unisex Sandals",
                        UnitPrice = 12.00,
                        CategoryID = 3
                    },
                        new Product
                    {
                        ProductID = 49,
                        ProductName = "Women's Bag",
                        UnitPrice = 37.50,
                        CategoryID = 1
                    }
                };
            foreach (Product item in products)
            {
                ResourceResponse<Document> result = client.CreateDocumentAsync(collectionSelfLink, item).Result;
            }
        }

        /// <summary>
        /// Seed the categories of the product catalog in a Cosmos DB collection 
        /// </summary>
        /// <param name="client">A Cosmos DB account Client </param>
        /// <param name="collectionSelfLink2"> A link to a Cosmos DB collection</param>
        private static void SeedCategoriesInDB(DocumentClient client, Uri collectionSelfLink2)
        {
            List<Category> categories = new List<Category>
                {
                    new Category
                    {
                        CategoryID = 1,
                        CategoryName = "Women"
                    },
                    new Category
                    {
                        CategoryID = 2,
                        CategoryName = "Men"
                    },
                    new Category
                    {
                        CategoryID = 3,
                        CategoryName = "Unisex"
                    }
                };
            foreach (Category catg in categories)
            {
                ResourceResponse<Document> result = client.CreateDocumentAsync(collectionSelfLink2, catg).Result;
            }
        }
    }
}