//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved. 
// </copyright>
// <author>Devki Trivedi</author>
//-----------------------------------------------------------------------

namespace DataGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Class contains code that creates randomized data from a clothing 
    /// store and inserts it into a Cosmos DB collection.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Singleton instance of the Cosmos DB client that accesses the service.
        /// </summary>
        private static readonly DocumentClient Client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["endpoint"]),
            ConfigurationManager.AppSettings["authKey"],
            new ConnectionPolicy()
            {
                UserAgentSuffix = " samples-net/3",
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            });

        /// <summary>
        /// Initializes Uri for the Cosmos DB collection.
        /// </summary>
        private static readonly Uri CollectionUri = UriFactory.CreateDocumentCollectionUri(
            ConfigurationManager.AppSettings["database"],
            ConfigurationManager.AppSettings["collection"]);

        /// <summary>
        /// Contains the valid actions a user can take.
        /// </summary>
        public enum Action
        {
            /// <summary>
            /// User has viewed an item.
            /// </summary>
            Viewed,

            /// <summary>
            /// User has added an item to cart. 
            /// </summary>
            Added,

            /// <summary>
            /// User has purchased an item.
            /// </summary>
            Purchased
        }

        /// <summary>
        /// Main method that calls CreateData().
        /// </summary>
        /// <param name="args"> Default main arguments. </param>
        public static void Main(string[] args)
        {
            CreateData();
            Console.ReadKey();
            return;
        }

        /// <summary>
        /// Randomly creates an Action using Randomizer from the Bogus library to generate a number between
        /// 1 and 3 and matches it with an Action.
        /// </summary>
        /// <param name="rand"> An instance of Randomizer from the Bogus library. </param>
        /// <returns> Returns a valid type of Action. </returns>
        public static Action GetRandomAction(Randomizer rand)
        {
            int actionIndex = rand.Number(0, 2);
            switch (actionIndex)
            {
                case 0:
                    return Action.Viewed;
                case 1:
                    return Action.Added;
                case 2:
                    return Action.Purchased;
                default:
                    throw new Exception($"Oops! Unexpected index. Index: {actionIndex}");
            }
        }

        /// <summary>
        /// Method that creates randomized data by generating a random number for the CartID, selecting a 
        /// random item from the list of items, and matching it with a random Action from GetRandomAction(Randomizer r).
        /// </summary>
        public static async void CreateData()
        {
            Randomizer random = new Randomizer();
            string[] items = new string[]
            {
                "Unisex Socks", "Women's Earring", "Women's Necklace", "Unisex Beanie",
                "Men's Baseball Hat", "Unisex Gloves", "Women's Flip Flop Shoes", "Women's Silver Necklace",
                "Men's Black Tee", "Men's Black Hoodie", "Women's Blue Sweater", "Women's Sweatpants",
                "Men's Athletic Shorts", "Women's Athletic Shorts", "Women's White Sweater", "Women's Green Sweater",
                "Men's Windbreaker Jacket", "Women's Sandal", "Women's Rainjacket", "Women's Denim Shorts",
                "Men's Fleece Jacket", "Women's Denim Jacket", "Men's Walking Shoes", "Women's Crewneck Sweater",
                "Men's Button-Up Shirt", "Women's Flannel Shirt", "Women's Light Jeans", "Men's Jeans",
                "Women's Dark Jeans", "Women's Red Top", "Men's White Shirt", "Women's Pant", "Women's Blazer Jacket", "Men's Puffy Jacket",
                "Women's Puffy Jacket", "Women's Athletic Shoes", "Men's Athletic Shoes", "Women's Black Dress", "Men's Suit Jacket", "Men's Suit Pant",
                "Women's High Heel Shoe", "Women's Cardigan Sweater", "Men's Dress Shoes", "Unisex Puffy Jacket", "Women's Red Dress", "Unisex Scarf",
                "Women's White Dress", "Unisex Sandals", "Women's Bag"
            };

            double[] prices = new double[]
            {

               3.75, 8.00, 12.00, 10.00,
                17.00, 20.00, 14.00, 15.50,
                9.00, 25.00, 27.00, 21.00, 22.50,
                22.50, 32.00, 30.00, 49.99, 35.50,
                55.00, 50.00, 65.00, 31.99, 79.99,
                22.00, 19.99, 19.99, 80.00, 85.00,
                90.00, 33.00, 25.20, 40.00, 87.50, 99.99,
                95.99, 75.00, 70.00, 65.00, 92.00, 95.00,
                72.00, 25.00, 120.00, 105.00, 130.00, 29.99,
                84.99, 12.00, 37.50
            };

           bool loop = true;
           while (loop)
            {
                int itemIndex = random.Number(0,48);
                Event e = new Event()
                {
                    CartID = random.Number(1000, 9999),
                    Action = GetRandomAction(random),
                    Item = items[itemIndex],
                    Price = prices[itemIndex]
                };
                await InsertData(e);

                List<Action> previousActions = new List<Action>();
                switch (e.Action)
                {
                    case Action.Viewed:
                        break;
                    case Action.Added:
                        previousActions.Add(Action.Viewed);
                        break;
                    case Action.Purchased:
                        previousActions.Add(Action.Viewed);
                        previousActions.Add(Action.Added);
                        break;
                    default:
                        break;
                }

                foreach (Action previousAction in previousActions)
                {
                    Event previousEvent = new Event()
                    {
                        CartID = e.CartID,
                        Action = previousAction,
                        Item = e.Item,
                        Price = e.Price
                    };
                    await InsertData(previousEvent);
                }
                //int time = random.Number(0, 500);
                //System.Threading.Thread.Sleep(time);
                
            }
            string key = Console.ReadKey().Key.ToString();
            if (key == " ")
                loop = false;
            else
                loop = true;
            CreateData();
        }

        /// <summary>
        /// Inserts each event e to the database by using Azure DocumentClient library.
        /// </summary>
        /// <param name="e"> An instance of the Event class representing a user click. </param>/
        private static async Task InsertData(Event e)
        {
            await Client.CreateDocumentAsync(CollectionUri, e);
            Console.Write("*");
        }

        /// <summary>
        /// Class that defines the parameters of an event a user can make.
        /// </summary>
        internal class Event
        {
            /// <summary>
            /// Gets or sets an ID to represent the user that is shopping.
            /// </summary>
            public int CartID { get; set; }

            /// <summary>
            /// Gets or sets action from the action list.
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            public Action Action { get; set; }

            /// <summary>
            /// Gets or sets item from the item list.
            /// </summary>
            public string Item { get; set; }

            /// <summary>
            /// Gets or sets price associated with each Item by index from the price list.
            /// </summary>
            public double Price { get; set; }
        }
    }
}