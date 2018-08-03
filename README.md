# Azure Cosmos DB - Change Feed Lab
    
####  The Azure Cosmos DB change feed is a mechanism for getting a continuous and incremental feed of records from a Cosmos DB container as those records are being created or modified.   
   
#### In this lab, you'll focus on how a company can use the change feed feature to its advantage and understand user patterns with *real-time data analysis visualization*. You will approach the change feed from the perspective of an e-commerce company and work with a collection of events, i.e. records that represent occurrences such as a user viewing an item, adding an item to their cart, or purchasing an item. When one of these events occurs and a new record is consequently created, the change feed will log that record. The change feed will then trigger a series of steps resulting in visualization of metrics analyzing company performance and site activity.
   
####  
New to Azure Cosmos DB? We recommend checking out [Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction "Cosmos DB")

New to the Cosmos DB change feed feature? We recommend checking out [Change Feed](https://azure.microsoft.com/en-us/resources/videos/azure-cosmosdb-change-feed/ "Change Feed")

![ProjectDiagram](/Lab/labpics/ProjectVisual.PNG)

This lab will proceed as follows:
1. **Data Generation**: You will simulate data that represents events such as a user viewing an item, adding an item to their cart, and purchasing an item. You will do this in two ways.      
   
	(1) Randomized data generator to simulate events in bulk so that you have a large set of sample data   
	(2) E-commerce web site you will run where you can view items from a product catalog, add them to your cart, and purchase the items in your cart. When you perform any of those actions, records will be created on Cosmos DB.   
   
   Below is an example of a simulated event record:

		{      
			"CartID": 2486,
			"Action": "Viewed",
			"Item": "Women's Denim Jacket",
			"Price": 31.99
		}

2. **Cosmos DB**: Cosmos DB will take in the generated data and store it in a collection.   
3. **Change Feed**: The change feed will listen for changes to the Cosmos DB collection. Every time a new document is added into the collection (i.e. an event occurs such a user viewing an item, adding an item to their cart, or purchasing an item), the change feed will trigger an [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/ "Azure Function").   
4. **Azure Function**: The Azure Function will process the new data and send it into an [Azure Event Hub](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about "Azure Event Hub").
5. **Event Hub**: The Azure Event Hub will store these events and send them to [Azure Stream Analytics](https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-introduction "Azure Stream Analytics").
6. **Azure Stream Analytics**: You will write queries by which Azure Stream Analytics will process events and build a real-time data analysis. It will send that analysis to [Microsoft Power BI](https://docs.microsoft.com/en-us/power-bi/ "Microsoft Power BI").
7. **Power BI**: You will use Microsoft Power BI to visualize the data analysis from Azure Stream Analytics and build a dashboard so you can see your metrics change in real time. Sample metrics that you can visualize include revenue, unique site visitors, most popular items, and average price of those items that are viewed versus added to a cart versus purchased. These sample metrics can help an e-commerce company evaluate its site popularity, develop its advertising and pricing strategies, and make decisions regarding what inventory to invest in.

## <a name="prerequisites"></a>Prerequisites
- 64-bit Windows 10 Operating System  
- Microsoft .NET Framework 4.7.1 or higher  
- Microsoft .NET Core 2.1 (or higher) SDK   
- Visual Studio with Universal Windows Platform development, .NET desktop development, and ASP.NET and web development workloads
- Microsoft Azure Subscription  
- Microsoft Power BI Account 

--- 

[Pre-Lab: Creating Azure Cosmos DB Resources](#prelab)  
[Part 1: Creating a Database and Collection To Load Data Into](#part1)   
[Part 2: Creating a Leases Collection for Change Feed Processing](#part2)   
[Part 3: Getting Storage Account Key and Connection String](#part3)  
[Part 4: Getting Event Hub Namespace Connection String](#part4)  
[Part 5: Setting Up Azure Function with Cosmos DB Account](#part5)  
[Part 6: Inserting Simulated Data into Cosmos DB in Real Time](#part6)  
[Part 7: Setting Up Azure Stream Analytics and Data Analysis Visualization](#part7)  
[Part 8: Connecting to PowerBI](#part8)   
[Part 9: Visualizing with a Real E-Commerce Site (OPTIONAL)](#part9)


## <a name="prelab"></a>Pre-Lab: Creating Azure Cosmos DB Resources

You will be using [Azure Resource Manager](https://docs.microsoft.com/en-us/azure/azure-resource-manager/) to create the Azure resources necessary for the lab. 

1. Make sure that you have an **Unrestricted Execution Policy** on your **Windows PowerShell**. To do so, open **Windows PowerShell as an Administrator** and run the following commands:  

    `Get-ExecutionPolicy  `  
    
    `Set-ExecutionPolicy Bypass  `

2. Exit Windows PowerShell.  

2. Clone this repository to your computer.  

3. Open the repository that you just cloned, then navigate to the **Azure Resource Manager** folder, and open the file called **parameters.json** in **Visual Studio**.  

4. Fill in the blanks as indicated in **parameters.json**. You'll need to use the names that you give to each of your resources later. We recommend keeping this window available for your reference later in the lab.

5. Open **Windows PowerShell** again, then navigate to the **Azure Resource Manager** folder and run the following command:  
    ` .\deploy.ps1 `  

6. Once prompted, enter the **Subcription ID** for your Azure subscription (available on [Azure Portal ](http://portal.azure.com "Azure Portal ")), **"changefeedlab"** for your resource group name, and **"run1"** for your deployment name.   
   
	**Note:** If you are having trouble finding your **Subscription ID**, go to the [Azure Portal](http://portal.azure.com "Azure Portal") and search for **Subscriptions**, then select your subscription. You will see the **Subscription ID** in your subscription **Overview**.   
   
7. The resources will begin to deploy. This may take up to 10 minutes.

---

## <a name="part1"></a>Part 1: Creating A Database and Collection To Load Data Into 
An Azure Cosmos DB collection is a container for data records. You will now create a collection to hold e-commerce site events. You are building this collection so that, when a user views an item, adds an item to their cart, or purchases an item, the collection will receive a record that includes the action ("viewed", "added", or "purchased"), the name of the item involved, the price of the item involved, and the ID number of the user cart involved. 

1. Navigate to [Azure Portal](http://portal.azure.com "Azure Portal "). Then search for and select your **Azure Cosmos DB Account** that you created and named in the prelab.

	![collection1](/Lab/labpics/collection1.png)  

2. Navigate to **Overview** from the menu on the left-hand side.  
	![collection2](/Lab/labpics/collection2.PNG)  


3. Select **New Collection** from the menu on the top of the page. Then perform the following actions:   
	a. In the **Database id** field, select **Create new**, then enter **database1**. Leave the **Provision database throughput** box unchecked.   
	b. In the **Collection id** field, enter **collection1**.  
    c. For **Storage capacity**, select **Unlimited**.  
    d. In the **Partition key** field, enter **/Item**. This is case-sensitive, so please make sure you enter it correctly.   
    e. In the **Throughput** field, enter **10000**.   
	f. Click the **OK** button.  
	
	![collection4](/Lab/labpics/collection4.png)  

4. Select **Keys** from the menu on the left-hand side. Take the PRIMARY CONNECTION STRING and copy it to a notepad or another document that you will have access to throughout the lab. You should label it **Cosmos DB Connection String**. You'll need to copy the string into your code later, so please take note and remember where you  are storing it.
	
	![collection6](/Lab/labpics/collection6.png)  

---

## <a name="part2"></a>Part 2: Creating A Leases Collection for Change Feed Processing
The lease collection coordinates processing the change feed across multiple workers. A separate collection is used to store the leases with one lease per partition.

1. Return to **Data Explorer** and select **New Collection** from the menu on the top of the page. Then perform the following actions:   
	a. In the **Database id** field, select **Use existing**, then enter **database1**.  
	b. In the **Collection id** field, enter **leases**.  
    c. For **Storage capacity**, select **Fixed**.   
	d. Leave the **Throughput** field set to its default value.  
	e. Click the **OK** button.  

---

## <a name="part3"></a>Part 3: Getting Storage Account Key and Connection String 
Azure Storage Accounts allow users to store data. In this lab, you will use a storage account to store data for the Azure Function. The Azure Function is triggered when any modification is made to the collection you created in Part 1, and in order for the Azure Function to do its job, it needs a place to store its work.

1. Return to your resource group and open the **Storage Account** that you created and named in the prelab.

2. Select **Access keys** from 
   the menu on the left-hand side.

	![storage3](/Lab/labpics/storage3.png)  

3. Take the values under **key 1** and copy them to a notepad or another document that you will have access to throughout the lab. You should label the **Key** as **Storage Key** and the **Connection string** as **Storage Connection String**. You'll need to copy these strings into your code later, so please take note and remember where you  are storing them.

	![storage4](/Lab/labpics/storage4.png)

---
 
## <a name="part4"></a>Part 4: Getting Event Hub Namespace Connection String
An Azure Event Hub is exactly what it sounds like. It receives event data, stores event data, processes event data, and forwards event data. In this lab, the Event Hub will receive a document every time a new event occurs (i.e. an item is viewed by a user, added to a user's cart, or purchased by a user) and then will forward that document to Azure Stream Analytics.

1. Return to your resource group and open the **Event Hub Namespace** that you created and named in the prelab .
   
2. Select **Shared access policies** from the menu on the left-hand side.
	
	![eventhub7](/Lab/labpics/eventhub7.png)  

3. Select **RootManageSharedAccessKey**. Take the **Connection string-primary key** and copy it to a notepad or another document that you will have access to throughout the lab. You should label it **Event Hub Namespace connection string**. You'll need to copy the string into your code later, so please take note and remember where you  are storing it.
	
	![eventhub8](/Lab/labpics/eventhub8.png)  

---

## <a name="part5"></a>Part 5: Setting Up Azure Function with Cosmos DB Account
When a new document is created or modifications are made to a current document in a Cosmos DB collection, the change feed automatically adds that modified document to its history of collection changes. You will now build and run an Azure Function that processes the change feed. When a document is created or modified in the collection you created, the Azure Function will be triggered by the change feed. Then the Azure Function will send the modified document to the Event Hub.   

1. Return to the repository that you cloned on your device.
2. Right-click the file named **ChangeFeedLabSolution.sln** and select **Open With Visual Studio**.
3. Navigate to **local.settings.json** in Visual Studio. Then use the values you recorded earlier to fill in the blanks.
4. Navigate to **ChangeFeedProcessor.cs**. In the parameters for the **Run** function, perform the following actions:   
   
	a. Replace the text **YOUR COLLECTION NAME HERE** with the name of your collection. If you followed earlier instructions, the name of your collection is **collection1**.   
   
	b. Replace the text **YOUR LEASES COLLECTION NAME HERE** with the name of your leases collection. If you followed earlier instructions, the name of your leases collection is **leases**.   
   
	c. At the top of Visual Studio, make sure that the Startup Project box on the left of the green arrow says **ChangeFeedFunction**, and if it says something else, arrow down and click on **ChangeFeedFunction**.   
   
	d. Press the start button at the top of the page to run the program. It will look like this green triangle:
	![startbutton](/Lab/labpics/startbutton.PNG)   
   
	e. You will know the function is running when the console app says "Job host started" at the bottom.
		 
---

## <a name="part6"></a>Part 6: Inserting Simulated Data into Cosmos DB in Real Time  
To see how the change feed processes new actions on an e-commerce site, you will simulate data that represents users viewing items from the product catalog, adding those items to their carts, 
and purchasing the items in their carts. This data is arbitrary and for the purpose of replicating what data on an Ecommerce site would look like. 

1. Navigate back to the repository in File Explorer, and right-click **ChangeFeedFunction.sln** to open it **again** in a new Visual Studio window.   
   
   **Note:** Yes, we know it may seem weird to have the same solution open in two windows! But it is necessary so that you can run two programs within the solution at the same time.
	
	![data1](/Lab/labpics/data1.png)  

2. Navigate to [Azure Portal ](http://portal.azure.com "Azure Portal ").
3.  Navigate to the resource group that you created during the prelab, and then navigate to the **Azure Cosmos DB account** that you created during the prelab.
4. Select **Keys**.   
	
	![data5](/Lab/labpics/data5.png)  

5. Take the **URI** and **PRIMARY KEY** and copy it to a notepad or another document that you will have access to throughout the lab. You should label them **URI** and **PRIMARY KEY**. You'll need to copy them into your code later, so please take note and remember where you  are storing them.
	
	![codes](/Lab/labpics/data13.PNG)   

6. Return to **Visual Studio**.  

7. Navigate to the **App.config** file.  

8. Within the `<appSettings>` block, add the **URI** and unique **PRIMARY KEY** that you just saved. 

9. Add in the **collection** and **database** names. (These names should be **collection1** and **database1** unless you chose to name yours differently.)
	
	![data6](/Lab/labpics/data6.png)  

10. Save the changes on all the files edited.   

11. At the top of Visual Studio, make sure that the Startup Project box on the left of the green arrow says **DataGenerator**, and if it says something else, arrow down and click on **DataGenerator**.  
	Then press the start button at the top of the page to run the program. 
	
	![data7](/Lab/labpics/data7.png)  

12. Wait for the program to run. The stars mean that data is coming in! Keep the program running - it is important that lots of data is collected.  
	
	![data8](/Lab/labpics/data8.PNG)  


13. If you navigate to [Azure Portal ](http://portal.azure.com "Azure Portal "), then to the Cosmos DB account within your resource group, then to **Data Explorer**, you will see the randomized data imported in your collection! Your collection is **collection1** unless you chose to name yours differently.  
	
	![data11](/Lab/labpics/data12.png)
	
---

## <a name="part7"></a>Part 7: Setting Up Azure Stream Analytics and Data Analysis Visualization
Azure Stream Analytics is a fully managed cloud service for real-time processing of streaming data. In this lab, you will use ASA to process new events from the Event Hub (i.e. when an item is viewed, added to a cart, or purchased), incorporate those events into real-time data analysis, and send them into Power BI for visualization.

1. On [**Azure Portal**](http://portal.azure.com "Azure Portal "), navigate to your resource group, then to **streamjob1** (the stream analytics job that you created in the prelab).   

2. Select **Inputs** as demonstrated below.
	
	![asa3](/Lab/labpics/asa3.png)  

3. Select **+ Add stream input**. Then select **Event Hub** from the drop-down menu.
	
	![asa4](/Lab/labpics/asa4.png)
4. To add a new input to the stream analytics job, perform the following actions:  
    	a. In the **Input** alias field, enter **input**.   
		b. Select the option for **Select Event Hub from your subscriptions**.  
    	c. Set the **Subscription** field to your subscription.  
    	d. In the **Event Hub namespace** field, enter the name of your Event Hub Namespace that you created during the prelab.   
		e. In the **Event Hub name** field, select the option for **Use existing** and choose **event-hub1** from the drop-down menu.  
    	f. Leave **Event Hub policy name** field set to its default value.  
    	g. Leave **Event serialization format** as **JSON**.  
    	h. Leave **Encoding field** set to **UTF-8**.  
    	i. Leave **Event compression type** field set to **None**.  
		j. Click the **Save** button.
	
	![PowerBIInput](/Lab/labpics/PowerBIInput.PNG)

5. Navigate back to the stream analytics job page, and select **Outputs** as demonstrated below.
	
	![asa5](/Lab/labpics/asa5.png)
	
6. Select **+ Add**. Then select  **Power BI** from the drop-down menu.
	
	![asa6](/Lab/labpics/asa6.png)

7. To create a new Power BI output to visualize average price, perform the following actions:  
    	a. In the **Output alias** field, enter **averagePriceOutput**.  
    	b. Leave the **Group workspace** field set to **Authorize connection to load workspaces**.   
		c. In the **Dataset name** field, enter **averagePrice**.  
    	d. In the **Table name** field, enter **averagePrice**.  
    	e. Click the **Authorize** button, then follow the instructions to authorize the connection to Power BI.  
		f. Click the **Save** button.      

8. Then go back to **streamjob1** and click **Edit query**.

	![asa8](/Lab/labpics/asa8.png)

9. Paste the following query into the query window. Then click **Save** in the upper left-hand corner.   

	
		/*AVERAGE PRICE*/      
		SELECT System.TimeStamp AS Time, Action, AVG(Price)  
		INTO averagePriceOutput  
		FROM input  
		GROUP BY Action, TumblingWindow(second,5) 

	**Note:**   
	The **AVERAGE PRICE** query calculates the average price of all items that are viewed by users, the average price of all items 	that are added to users' carts, and the average price of all items that are purchased by users. This metric can help e-commerce companies decide what prices to sell items at and what inventory to invest in. For example, if the average price of items viewed is much higher than the average price of items purchased, then a company might choose to add less expensive items to its inventory.  

10. Now return to **streamjob1** and click the **Start** button at the top of the page. Azure Stream Analytics can take a few minutes to start up, but eventually you will see it change from "Starting" to "Running".

---

## <a name="part8"></a>Part 8: Connecting to PowerBI
Power BI is a suite of business analytics tools to analyze data and share insights. It's a great example of how you can strategically visualize the data analysis that you coded in the last section.

1. Sign in to [Power BI](https://docs.microsoft.com/en-us/power-bi/ "Power BI") and navigate to **My Workspace** by opening the menu on the left-hand side of the page.

2. Click **+ Create** in the top right-hand corner and then select **Dashboard** to create a dashboard.

3. Click **+ Add tile** in the top right-hand corner.

	![powerbi1](/Lab/labpics/powerbi1.png)

4. Select **Custom Streaming Data**, then click the **Next** button.
	
	![powerbi2](/Lab/labpics/powerbi2.png)

5. Select **averagePrice** from **YOUR DATASETS**, then click **Next**.   
   
6. In the **Visualization Type** field, choose **Clustered bar chart** from the drop-down menu.   
	Under **Axis**, add **action**.   
	Skip **Legend** without adding anything.   
	Then, under the next section called **Value**, add **avg**.   
	Click **Next**, then title your chart, and click **Apply**. You should see a new chart on your dashboard!

7. Now, if you want to visualize more metrics **(optional)**, you can go back to **streamjob1** and create three more outputs with the following fields.   
		a. Output alias: incomingRevenueOutput, Dataset name: incomingRevenue, Table name: incomingRevenue  
		b. Output alias: top5Output, Dataset name: top5, Table name: top5   
		c. Output alias: uniqueVisitorCountOutput, Dataset name: uniqueVisitorCount, Table name: uniqueVisitorCount  
		   
	Then click **Edit query** and paste the following queries **above** the one you already wrote.   

		/*TOP 5*/
		WITH Counter AS
		(
		SELECT Item, Price, Action, COUNT(*) AS countEvents
		FROM input
		WHERE Action = 'Purchased'
		GROUP BY Item, Price, Action, TumblingWindow(second,30)
		), 
		top5 AS
		(
		SELECT DISTINCT
		CollectTop(5)  OVER (ORDER BY countEvents) AS topEvent
		FROM Counter
		GROUP BY TumblingWindow(second,30)
		), 
		arrayselect AS 
		(
		SELECT arrayElement.ArrayValue
		FROM top5
		CROSS APPLY GetArrayElements(top5.topevent) AS arrayElement
		) 
		SELECT arrayvalue.value.item, arrayvalue.value.price, arrayvalue.value.countEvents
		INTO top5Output
		FROM arrayselect

		/*REVENUE*/
		SELECT System.TimeStamp AS Time, SUM(Price)
		INTO incomingRevenueOutput
		FROM input
		WHERE Action = 'Purchased'
		GROUP BY TumblingWindow(hour, 1)

		/*UNIQUE VISITORS*/
		SELECT System.TimeStamp AS Time, COUNT(DISTINCT CartID) as uniqueVisitors
		INTO uniqueVisitorCountOutput
		FROM input
		GROUP BY TumblingWindow(second, 5)

	**Note:**   
	The **TOP 5** query calculates the top 5 items, ranked by the number of times that they have been purchased. This metric can help e-commerce companies evaluate which items are most popular and can influence the company's advertising, pricing, and inventory decisions.   
				
	The **REVENUE** query calculates revenue by summing up the prices of all items purchased each minute. This metric can help e-commerce companies evaluate its financial performance and also understand what times of day are most revenue-driving. This can impact overall company strategy, marketing in particular.   
				
	The **UNIQUE VISITORS** query calculates how many unique visitors are on the site every 5 seconds by detecting unique cart ID's. This metric can help e-commerce companies evaluate their site activity and strategize how to acquire more customers.
					
8. You can now add tiles for these datasets as well.   
	For Top 5, it would make sense to do a clustered column chart with the items as the axis and the count as the value.   
	For Revenue, it would make sense to do a line chart with time as the axis and the sum of the prices as the value. The time window to display should be the largest possible in order to deliver as much information as possible.   
	For Unique Visitors, it would make sense to do a card visualization with the number of unique visitors as the value.   
   
   This is how our dashboard looks with these charts:   
	![powerbisnap](/Lab/labpics/powerbisnap.png)
--- 

## <a name="part9"></a>Part 9: Visualizing with a Real E-commerce Site (OPTIONAL)
You will now observe how you can use your new data analysis tool to connect with a real e-commerce site.   

In order to build the e-commerce site, you'll use a Cosmos DB database to store the list of product categories (Women's, Men's, Unisex), the product catalog, and a list of the most popular items.   
   
1. Navigate back to the **Azure Portal**, then to your **Cosmos DB account**, then to **Data Explorer**.   
Add two collections under **database1** named **products** and **categories** with **Fixed** storage capacity.   
Add another collection under **database1** named **topItems** with **Unlimited** storage capacity. Write **/Item** as the partition key.   
2. Click on the **topItems** collection, and under **Scale and Settings** set the **Time to Live** to be **30 seconds** so that topItems updates every 30 seconds.  

	![webapp1](/Lab/labpics/webapp15.PNG)  

3. In order to populate the **topItems** collection with the most frequently purchased items, navigate back to **streamjob1** and add a new **Output**. Select **Cosmos DB**.

	![webapp1](/Lab/labpics/webapp12.PNG) 


4. Fill in the required fields as pictured below.

	![webapp1](/Lab/labpics/webapp13.PNG) 

5. If you added the optional **TOP 5** query in the previous part of the lab, proceed to part **5a**.   
If not, proceed to part **5b**.   
   
	5a. In **streamjob1**, select **Edit query** and paste the following query in your Azure Stream Analytics query editor **below the TOP 5 query but above the rest of the queries**.
	 

		
		SELECT arrayvalue.value.item AS Item, arrayvalue.value.price, arrayvalue.value.countEvents
		INTO topItems
		FROM arrayselect


   
	5b. In **streamjob1**, select **Edit query** and paste the following query in your Azure Stream Analytics query editor **above all other queries**.
	 

		/*TOP 5*/
		WITH Counter AS
		(
		SELECT Item, Price, Action, COUNT(*) AS countEvents
		FROM input
		WHERE Action = 'Purchased'
		GROUP BY Item, Price, Action, TumblingWindow(second,30)
		), 
		top5 AS
		(
		SELECT DISTINCT
		CollectTop(5)  OVER (ORDER BY countEvents) AS topEvent
		FROM Counter
		GROUP BY TumblingWindow(second,30)
		), 
		arrayselect AS 
		(
		SELECT arrayElement.ArrayValue
		FROM top5
		CROSS APPLY GetArrayElements(top5.topevent) AS arrayElement
		) 
		SELECT arrayvalue.value.item AS Item, arrayvalue.value.price, arrayvalue.value.countEvents
		INTO topItems
		FROM arrayselect



6. Open **EcommerceWebApp.sln** and navigate to the **Web.config** file in the **Solution Explorer**.

	![webapp1](/Lab/labpics/webapp1.PNG)  

7. Within the `<appSettings>` block, add the **URI** and **PRIMARY KEY** that you saved earlier where it says **your URI here** and **your primary key here**.   
   
	Then add in your **database name** and **collection name** as indicated. (These names should be **database1** and **collection1** unless you chose to name yours differently.)   
   
	Fill in your **products collection name**, **categories collection name**, and **top items collection name** as indicated. (These names should be **products**, **categories**, and **topItems** unless you chose to name yours differently.)


8. Navigate to and open the **Checkout folder** within **EcommerceWebApp.sln**. Then open the **Web.config** file within that folder.

	![webapp3](/Lab/labpics/webapp3.PNG)  

9. Within the `<appSettings>` block, add the **URI** and **PRIMARY KEY** that you saved earlier where indicated.   
Then add in your **databse name** and **collection name** as indicated. (These names should be **database1** and **collection1** unless you chose to name yours differently.)
 
	![webapp4](/Lab/labpics/webapp4.PNG)  


10. Press the green triangle start button at the top of the page to run the program.  

	![startbutton](/Lab/labpics/startbutton.PNG)  


11. Now you can play around on the e-commerce site. When you view an item, add an item to your cart, change the quantity of an item in your cart, or purchase an item, these events will be passed through the Cosmos DB change feed to Event Hub, ASA, and then Power BI. We recommend continuing to run DataGenerator to generate significant web traffic data and provide a realistic set of "Hot Products" on the e-commerce site. 
   
To delete the resources that you created during this lab, navigate to the resource group on [Azure Portal](http://portal.azure.com "Azure Portal "), then select **Delete resource group** from the menu at the top of the page and follow the instructions provided.

#### This is just one example of how the change feed can be used by Cosmos DB customers.   

Feedback? Let us know [here!](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR8K_JtWBhupCpepG-HuzNVlUOUdNU0E5VUNRMlpNU1pNTzRCVE1VNE0wOS4u)

#### For more information about what's happening at Azure Cosmos DB, check out the [Cosmos DB Blog](https://azure.microsoft.com/en-us/blog/tag/cosmos-db/ "Cosmos DB Blog")!
