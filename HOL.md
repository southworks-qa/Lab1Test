<a name="Title" />
# Introduction to Windows Azure #

---
<a name="Overview" />
## Overview ##

A service hosted in Windows Azure consists of one or more web roles and worker roles. A web role is an ASP.NET Web application accessible via an HTTP or HTTPS endpoint and is commonly the front-end for an application. Worker roles are background-processing applications and are typically found in the back-end. Windows Azure services may be comprised of one or both types of roles and can run multiple instances of each type. Role instances can be added or removed based on demand and allow applications to quickly and economically scale-up or down when the need arises.

Windows Azure storage services provide storage in the cloud, which includes Blob services for storing text and binary data, Table services for structured storage that can be queried, and Queue services for reliable and persistent messaging between services.

In this hands-on lab, you will explore the basic elements of a Windows Azure service by creating a simple GuestBook application that demonstrates many features of Windows Azure, including web and worker roles, blob storage, table storage, and queues. 

In the GuestBook application, a web role provides the front-end that allows users to view the contents of the guest book and submit new entries. Each entry contains a name, a message, and an associated picture. The application also contains a worker role that can generate thumbnails for the images that users submit.

When users post a new item, the web role uploads the picture to blob storage and creates an entry in table storage that contains the information entered by the user and a link to the blob with the picture. The web role renders this information to the browser so users can view the contents of the guest book.

After storing the image and creating the entry, the web role posts a work item to a queue to have the image processed. The worker role fetches the work item from the queue, retrieves the image from blob storage, and resizes it to create a thumbnail. Using queues to post work items is a common pattern in cloud applications and enables the separation of compute-bound tasks from the front-end. The advantage of this approach is that front and back ends can be scaled independently.

<a name="Objectives" />
### Objectives ###

In this hands-on lab, you will learn how to:

* Create applications in Windows Azure using web roles and worker roles
* Use Windows Azure storage services including blobs, queues and tables
* Publish an application to Windows Azure

<a name="Prerequisites" />
### Prerequisites ###

The following is required to complete this hands-on lab:

* IIS 7 (with ASP.NET, WCF HTTP Activation)

* [Microsoft Visual Studio 2010][1]

* [Microsoft.NET Framework 4.0][2]

* [Windows Azure Tools for Microsoft Visual Studio 1.6][3]

* [SQL Server 2008 Express Edition (or later)][4]

[1]: http://msdn.microsoft.com/vstudio/products/
[2]: http://go.microsoft.com/fwlink/?linkid=186916
[3]: http://www.microsoft.com/windowsazure/sdk/
[4]: http://www.microsoft.com/express/sql/download/


<a name="Setup"/>
### Setup ###

In order to execute the exercises in this hands-on lab you need to set up your environment.

1. Open a Windows Explorer window and browse to the lab’s **source** folder.

1. Double-click the **Setup.cmd** file in this folder to launch the setup process that will configure your environment and install the Visual Studio code snippets for this lab.

1. If the User Account Control dialog is shown, confirm the action to proceed.

>**Note:** Make sure you have checked all the dependencies for this lab before running the setup.

<a name="UsingCodeSnippets"/>
### Using the Code Snippets ###

Throughout the lab document, you will be instructed to insert code blocks. For your convenience, most of that code is provided as Visual Studio Code Snippets, which you can use from within Visual Studio 2010 to avoid having to add it manually. 

---
<a name="Exercises"/>
## Exercises ##

This hands-on lab includes the following exercises:

* [Building Your First Windows Azure Application](#Exercise1)

* [Background Processing with Worker Roles and Queues](#Exercise2)

* [Publishing a Windows Azure Application](#Exercise3)


Estimated time to complete this lab: **75** minutes.

>**Note:** When you first start Visual Studio, you must select one of the predefined settings collections. Every predefined collection is designed to match a particular development style and determines window layouts, editor behavior, IntelliSense code snippets, and dialog box options. The procedures in this lab describe the actions necessary to accomplish a given task in Visual Studio when using the **General Development Settings** collection. If you choose a different settings collection for your development environment, there may be differences in these procedures that you need to take into account.

<a name="Exercise1" />
### Exercise 1: Building Your First Windows Azure Application ###

In this exercise, you create a guest book application and execute it in the local development fabric. For this purpose, you will use the Windows Azure Tools for Microsoft Visual Studio to create the project using the Cloud Service project template. These tools extend Visual Studio to enable the creation, building and running of Windows Azure services. You will continue to work with this project throughout the remainder of the lab.

>**Note:**  To reduce typing, you can right-click where you want to insert source code, select Insert Snippet, select My Code Snippets and then select the entry matching the current exercise step.

<a name="Ex1Task1" />
#### Task 1 – Creating the Visual Studio Project ####

In this task, you create a new Cloud Service project in Visual Studio.

1. Open Visual Studio as administrator from **Start | All Programs | Microsoft Visual Studio 2010** by right clicking the **Microsoft Visual Studio 2010** shortcut and choosing **Run as administrator**.

1. If the **User Account Control** dialog appears, click **Yes**.

1. From the **File** menu, choose **New** and then **Project**.  

1. In the **New Project dialog**, expand **Visual C#**. In the **Installed Templates** list and select **Cloud**. Choose the **Windows Azure Project** template, set the Name of the project to GuestBook, set the location to **\source\Ex1-BuildingYourFirstWindowsAzureApp**, change the solution name to **Begin**, and ensure that **Create directory for solution** is checked. Click **OK** to create the project.

	![Creating a new Windows Azure Cloud Service project](images/new-cloud-service-project.png?raw=true "Creating a new Windows Azure Cloud Service project")

	_Creating a new Windows Azure Cloud Service project_
				
	>**Note:**  Windows Azure supports the .NET Framework 4.0. If you use Visual Studio 2010 to create the project, you can select this version for the target framework and take advantage of its new features.

1. In the **New Windows Azure Project** dialog, inside the **Roles** panel, expand the **Visual C#** tab. Select **ASP.NET Web Role** from the list of available roles and click the arrow (**>**) to add an instance of this role to the solution. Before closing the dialog, select the new role in the right panel, click the pencil icon and rename the role as **GuestBook_WebRole**. Click **OK** to create the cloud service solution.

	![Assigning roles to a Cloud Service project](images/assigning-roles-cloud-service-project.png?raw=true "Assigning roles to a Cloud Service project")
	
	_Assigning roles to a Cloud Service project_

1. In **Solution Explorer**, review the structure of the created solution.
 
	![Solution Explorer showing the GuestBook application](images/solution-explorer-guestbook.png?raw=true "Solution Explorer showing the GuestBook application")

	_Solution Explorer showing the GuestBook application_

	>**Note**: The generated solution contains two separate projects. The first project, named **GuestBook**, holds the configuration for the web and worker roles that compose the cloud application. It includes the service definition file, **ServiceDefinition.csdef**, which contains metadata needed by the Windows Azure fabric to understand the requirements of your application, such as which roles are used, their trust level, the endpoints exposed by each role, the local storage requirements and the certificates used by the roles. The service definition also establishes configuration settings specific to the application. The service configuration files specify the number of instances to run for each role and sets the value of configuration settings defined in the service definition file. This separation between service definition and configuration allows you to update the settings of a running application by uploading a new service configuration file.

	>You can create many configuration files, each one intended for a specific scenario such as production, development, or QA, and select which to use when publishing the application. By default, Visual Studio creates two files **ServiceConfiguration.Local.cscfg** and **ServiceConfiguration.Cloud.cscfg**.

	>The Roles node in the cloud service project enables you to configure what roles the service includes (web, worker or both) as well as which projects to associate with these roles. Adding and configuring roles through the Roles node will update the **ServiceDefinition.csdef** and **ServiceConfiguration.cscfg** files.

	>The second project, named **GuestBook_WebRole**, is a standard ASP.NET Web Application project template modified for the Windows Azure environment. It contains an additional class that provides the entry point for the web role and contains methods to manage the initialization, starting, and stopping of the role.

<a name="Ex1Task2" />  
#### Task 2 – Creating a Data Model for Entities in Table Storage ####

The application stores guest book entries in Windows Azure Table storage. The Table service offers semi-structured storage in the form of tables that contain collections of entities. Entities have a primary key and a set of properties, where a property is a name, typed-value pair. 

In addition to the properties required by your model, every entity in Table Storage has two key properties: the **PartitionKey** and the **RowKey**. These properties together form the table's primary key and uniquely identify each entity in the table. Entities also have a **Timestamp** system property, which allows the service to keep track of when an entity was last modified. This field is intended for system use and should not be accessed by the application.
The Table Storage client API provides a **TableServiceEntity** class that defines the necessary properties. Although you can use the **TableServiceEntity** class as the base class for your entities, this is not required.

The Table service API is compliant with the REST API provided by [WCF Data Services](http://msdn.microsoft.com/en-us/library/cc668792.aspx) (formerly ADO.NET Data Services Framework) allowing you to use the [WCF Data Services Client Library](http://msdn.microsoft.com/en-us/library/cc668772.aspx) (formerly .NET Client Library) to work with data in Table Storage using .NET objects.

The Table service does not enforce any schema for tables making it possible for two entities in the same table to have different sets of properties. Nevertheless, the GuestBook application uses a fixed schema to store its data. 

In order to use the WCF Data Services Client Library to access data in table storage, you need to create a context class that derives from **TableServiceContext**, which itself derives from **DataServiceContext** in WCF Data Services. The Table Storage API allows applications to create the tables that they use from these context classes. For this to happen, the context class must expose each required table as a property of type **IQueryable\<SchemaClass\>**, where **SchemaClass** is the class that models the entities stored in the table. 

In this task, you model the schema of the entities stored by the GuestBook application and create a context class to use WCF Data Services to access the information in table storage. To complete the task, you create an object that can be data bound to data controls in ASP.NET and implements the basic data access operations: read, update, and delete.

1. Create a new project for the schema classes. To create the project, in the **File** menu, point to **Add** and then select **New Project**.

1. In the **Add New Project** dialog, expand the language of your choice under the **Installed Templates** tree view, select the **Windows** category, and then choose the **Class Library** project template. Set the name to _GuestBook_Data_, leave the proposed location inside the solution folder unchanged, and then click **OK**.

	![Creating a class library for GuestBook entities](images/creating-class-library-guestbook.png?raw=true "Creating a class library for GuestBook entities")

	_Creating a class library for GuestBook entities_
 
1. Delete the default class file generated by the class library template. To do this, right-click **Class1.cs** and choose **Delete**. Click **OK** in the confirmation dialog.

1. Add a reference to the .NET Client Library for WCF Data Services in the **GuestBook_Data project**. In **Solution Explorer**, right-click the **GuestBook_Data** project node, select **Add Reference**, click the **.NET** tab, select the **System.Data.Services.Client** component and click **OK**.

	![Adding a reference to the System.Data.Service.Client component](images/adding-reference-systemdataservice.png?raw=true "Adding a reference to the System Data Service Client component")

	_Adding a reference to the System.Data.Service.Client component_
  
1. Repeat the previous step to add a reference to the Windows Azure storage client API assembly, this time choosing the **Microsoft.WindowsAzure.StorageClient** component instead.

1. Before you can store an entity in a table, you must first define its schema. To do this, right-click **GuestBook_Data** in **Solution Explorer**, point to **Add** and select **Class**. In the **Add New Item** dialog, set the name to **GuestBookEntry.cs** and click **Add**.

	![Adding the GuestBookEntry class](images/adding-guestbookentry-class.png?raw=true "Adding the GuestBookEntry class")

	_Adding the GuestBookEntry class_
 
1. At the top of the file, insert the following namespace declaration to import the types contained in the **Microsoft.WindowsAzure.StorageClient** namespace.

	````C#
	using Microsoft.WindowsAzure.StorageClient;
	````

1. If not already opened, open the **GuestBookEntry.cs** file and then update the declaration of the **GuestBookEntry** class to make it public and derive from the **TableServiceEntity** class.
	
	````C#
	public class GuestBookEntry: Microsoft.WindowsAzure.StorageClient.TableServiceEntity
	{
	}
	````

	>**Note:** **TableServiceEntity** is a class found in the Storage Client API. This class defines the **PartititionKey**, **RowKey** and **TimeStamp** system properties required by every entity stored in a Windows Azure table. 
	
	>Together, the **PartitionKey** and **RowKey** define the **DataServiceKey** that uniquely identifies every entity within a table.

1. Add a default constructor to the **GuestBookEntry** class that initializes its **PartitionKey** and RowKey properties.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookEntry constructor_ – CS)
	
	````C#
	public GuestBookEntry()
	{            
  	  PartitionKey = DateTime.UtcNow.ToString("MMddyyyy");
	  // Row key allows sorting, so we make sure the rows come back in time order.
  	  RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
	}
	````
	
	>**Note:** To partition the data, the GuestBook application uses the date of the entry as the **PartitionKey**, which means that there will be a separate partition for each day of guest book entries. In general, you choose the value of the partition key to ensure load balancing of the data across storage nodes.

	>The **RowKey** is a reverse DateTime field with a GUID appended for uniqueness. Tables within partitions are sorted in RowKey order, so this will sort the tables into the correct order to be shown on the home page, with the newest entry shown at the top.

1. To complete the definition of the **GuestBookEntry** class, add properties for **Message**, **GuestName**, **PhotoUrl**, and **ThumbnailUrl** to hold information about the entry.

	(Code Snippet – _Introduction to Windows Azure - Ex1 Table Schema Properties_ – CS)

	```` C#
	public string Message { get; set; }
	public string GuestName { get; set; }
	public string PhotoUrl { get; set; }
	public string ThumbnailUrl { get; set; }
	````
		
1. Save the **GuestBookEntry.cs** file.

1. Next, you need to create the context class required to access the _GuestBook_ table using WCF Data Services. To do this, in **Solution Explorer**, right-click the **GuestBook_Data** project, point to **Add** and select **Class**. In the **Add New Item** dialog, set the **Name** to **GuestBookDataContext.cs** and click **Add**.

1. In the new class file, update the declaration of the new class to make it public and inherit the **TableServiceContext** class.
	
	````C#
	public class GuestBookDataContext: Microsoft.WindowsAzure.StorageClient.TableServiceContext
	{
	}
	````
		
1. Now, add a default constructor to initialize the base class with storage account information.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataContext Class_ – CS)
	
	````C#
	public class GuestBookDataContext 
	  : Microsoft.WindowsAzure.StorageClient.TableServiceContext
	{
	 public GuestBookDataContext(string baseAddress, Microsoft.WindowsAzure.StorageCredentials credentials) : base(baseAddress, credentials)
	  { }
	}
	````
	
	>**Note:** You can find the **TableServiceContext** class in the storage client API. This class derives from **DataServiceContext** in WCF Data Services and manages the credentials required to access your storage account as well as providing support for a retry policy for its operations.
	
1. Add a property to the **GuestBookDataContext** class to expose the **GuestBookEntry** table. To do this, insert the following (highlighted) code into the class. 

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookEntry Property_ – CS)

    ````C#
	public class GuestBookDataContext 
	  : Microsoft.WindowsAzure.StorageClient.TableServiceContext
	{
	  ...
	  public IQueryable<GuestBookEntry> GuestBookEntry
	  {
	    get
	    {
			return this.CreateQuery<GuestBookEntry>("GuestBookEntry");
	    }
	  }
	}
    ````
	
	>**Note:** You can use the **CreateTablesFromModel** method in the **CloudTableClient** class to create the tables needed by the application. When you supply a **DataServiceContext** (or **TableServiceContext**) derived class to this method, it locates any properties that return an **IQueryable\<T\>**, where the generic parameter **T** identifies the class that models the table schema, and creates a table in storage named after the property.
	
1. Finally, you need to implement an object that can be bound to data controls in ASP.NET.  In **Solution Explorer**, right-click **GuestBook_Data**, point to **Add**, and select **Class**. In the **Add New Item** dialog, set the name to **GuestBookDataSource.cs** and click **Add**.

1. In the new class file, add the following namespace declarations to import the types contained in the **Microsoft.WindowsAzure** and **Microsoft.WindowsAzure.StorageClient** namespaces.

	````C#
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.StorageClient;
	````

1. In the **GuestBookDataSource** class, make the class **public** and define member fields for the data context and the storage account information, as shown below.
	
	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource Fields_ – CS)

	````C#
	public class GuestBookDataSource
	{
	   private static CloudStorageAccount storageAccount;
	   private GuestBookDataContext context;
	}
	````

1. Now, add a static constructor to the data source class as shown in the following (highlighted) code. This code creates the tables from the **GuestBookDataContext** class.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource Static Constructor_ – CS)
	
	````C#
	public class GuestBookDataSource
	{
	  ...
	   static GuestBookDataSource()
	   {
	     storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
	
	     CloudTableClient.CreateTablesFromModel(
	     typeof(GuestBookDataContext),
	     storageAccount.TableEndpoint.AbsoluteUri,
	     storageAccount.Credentials);
	  }
	}
	````
		
	>**Note:**  The static  constructor initializes the storage account by reading its settings from the configuration and then uses the **CreateTablesFromModel** method in the **CloudTableClient** class to create the tables used by the application from the model defined by the **GuestBookDataContext** class. By using the static constructor, you ensure that this initialization task is executed only once.

1. Add a default constructor to the **GuestBookDataSource** class to initialize the data context class used to access table storage.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource Constructor_ – CS)
	
	```` C#
	public class GuestBookDataSource
	{
	  ...
	public GuestBookDataSource()
	{
	  this.context = new GuestBookDataContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
	  this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
	 }
	}
	````
	
1. Next, insert the following method to return the contents of the **GuestBookEntry** table. 
	
	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource Select_ – CS)
	
	````C#
	public class GuestBookDataSource
	{
	  ...
	   public IEnumerable<GuestBookEntry> GetGuestBookEntries()
	   {
	     var results = from g in this.context.GuestBookEntry
	                   where g.PartitionKey == DateTime.UtcNow.ToString("MMddyyyy")
	                   select g;
	     return results;
	   }
	}
	````
		
	>**Note:** The **GetGuestBookEntries** method retrieves today's guest book entries by constructing a LINQ statement that filters the retrieved information using the current date as the partition key value. The web role uses this method to bind to a data grid and display the guest book. 

1. Now, add the following method to insert new entries into the **GuestBookEntry** table.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource AddGuestBookEntry_ – CS)
	
	````C#
	public class GuestBookDataSource
	{
	  ...
	   public void AddGuestBookEntry(GuestBookEntry newItem)
	   {
	     this.context.AddObject("GuestBookEntry", newItem);
	     this.context.SaveChanges();
	   }
	}
	````
		
	>**Note:** This method adds a new GuestBookEntry object to the data context and then calls SaveChanges to write the entity to storage.
	
1. Finally, add a method to the data source class to update the **Thumbnail URL** property for an entry.

	(Code Snippet – _Introduction to Windows Azure - Ex1 GuestBookDataSource UpdateImageThumbnail_ – CS)
	
	````C#
	public class GuestBookDataSource
	{
	  ...
	   public void UpdateImageThumbnail(string partitionKey, string rowKey, string thumbUrl)
	   {
	      var results = from g in this.context.GuestBookEntry
	                  where g.PartitionKey == partitionKey && g.RowKey == rowKey
	                  select g;
	
	     var entry = results.FirstOrDefault<GuestBookEntry>();
	     entry.ThumbnailUrl = thumbUrl;
	     this.context.UpdateObject(entry);
	     this.context.SaveChanges();
	  }	
	}
	````
	
	>**Note:** The **UpdateImageThumbnail** method locates an entry using its partition key and row key; it updates the thumbnail URL, notifies the data context of the update, and then saves the changes.

1. Save the **GuestBookDataSource.cs** file.

<a name="Ex1Task3" />
#### Task 3 – Creating a Web Role to Display the Guest Book and Process User Input ####

In this task, you update the web role project that you generated in Task 1, when you created the Windows Azure Cloud Service solution. This involves updating the UI to render the list of guest book entries. For this purpose, you will find a page that has the necessary elements in the **Assets** folder of this exercise, which you will add to the project. Next, you implement the code necessary to store submitted entries in table storage and images in blob storage. To complete this task, you configure the storage account used by the Web role.

1. Add a reference in the web role to the **GuestBook_Data** project. In **Solution Explorer**, right-click the **GuestBook_WebRole** project node and select **Add Reference**, switch to the **Projects** tab, select the **GuestBook_Data** project, and then click then **OK**.

1. The web role template generates a default page. You will replace it with another page that contains the UI of the guest book application. To delete the page, in **Solution Explorer**, right-click **Default.aspx** in the **GuestBook_WebRole** project and select **Delete**.

1. Add the main page and its associated assets to the web role. To do this, right-click **GuestBook_WebRole** in **Solution Explorer**, point to **Add** and select **Existing Item**. In the **Add Existing Item** dialog, browse to the **Assets** folder in **\Source\Ex1-BuildingYourFirstWindowsAzureAppCS**, hold the **CTRL** key down while you select every file in this folder and click **Add**.

	>**Note:** The **Assets** folder contains five files that you need to add to the project, a Default.aspx file with its code-behind and designer files, a CSS file, and an image file.

1. Open the code-behind file for the main page in the **GuestBook_WebRole** project. To do this, right-click the **Default.aspx** file in **Solution Explorer** and select **View Code**.

1. In the code-behind file, insert the following namespace declarations.

	(Code Snippet – _Introduction to Windows Azure - Ex1  Web Role Namespace Declarations_  – CS)

	```` C#
	using System.IO;
	using System.Net;
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.ServiceRuntime;
	using Microsoft.WindowsAzure.StorageClient;
	using GuestBook_Data;
	````
		
1. Declare the following member fields in the **_Default** class.
	
	(Code Snippet – _Introduction to Windows Azure - Ex1  Web Role Member Fields_ – CS)
	
	````C#
	public partial class _Default : System.Web.UI.Page
	{
	   private static bool storageInitialized = false;
	   private static object gate = new Object();
	   private static CloudBlobClient blobStorage;	   
	   ...
	}
	````
		
1. Locate the **SignButton_Click** event handler in the code-behind file and insert the following code.
	
	(Code Snippet – _Introduction to Windows Azure - Ex1  SignButton_Click_ – CS)
	
	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  ...
	  protected void SignButton_Click(object sender, EventArgs e)
	  {
	    if (FileUpload1.HasFile)
	    {
	      InitializeStorage();
	
	      // upload the image to blob storage
	      string uniqueBlobName = string.Format("guestbookpics/image_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(FileUpload1.FileName));
	      CloudBlockBlob blob = blobStorage.GetBlockBlobReference(uniqueBlobName);
	      blob.Properties.ContentType = FileUpload1.PostedFile.ContentType;
	      blob.UploadFromStream(FileUpload1.FileContent);
	      System.Diagnostics.Trace.TraceInformation("Uploaded image '{0}' to blob storage as '{1}'", FileUpload1.FileName, uniqueBlobName);
	
	      // create a new entry in table storage
	      GuestBookEntry entry = new GuestBookEntry() { GuestName = NameTextBox.Text, Message = MessageTextBox.Text, PhotoUrl = blob.Uri.ToString(), ThumbnailUrl = blob.Uri.ToString() };
	      GuestBookDataSource ds = new GuestBookDataSource();
	      ds.AddGuestBookEntry(entry);
	      System.Diagnostics.Trace.TraceInformation("Added entry {0}-{1} in table storage for guest '{2}'", entry.PartitionKey, entry.RowKey, entry.GuestName);
	    }
	
	    NameTextBox.Text = "";
	    MessageTextBox.Text = "";
	
	    DataList1.DataBind();
	  }
	}
	````
		
	>**Note:** To process a new guest book entry after the user submits the page, the handler first calls the **InitializeStorage** method to ensure that the blob container used to store images exists and allows public access. You will implement this method shortly. 

	>It then obtains a reference to the blob container, generates a unique name and creates a new blob, and then uploads the image submitted by the user into this blob. Notice that the method initializes the **ContentType** property of the blob from the content type of the file submitted by the user. When the guest book page reads the blob back from storage, the response returns this content type, allowing a page to display the image contained in the blob simply by referring to its URL.

	>After that, it creates a new **GuestBookEntry** entity, which is the entity you defined in the previous task, initializes it with the information submitted by the user, and then uses the **GuestBookDataSource** class to save the entry to table storage using the .NET Client Library for WCF Data Services.
	
	>Finally, it data binds the guest book entries list to refresh its contents.

1. Update the body of the **Timer1_Tick** method with the code shown below. 

	(Code Snippet – _Introduction to Windows Azure - Ex1 Timer1_Tick_ – CS)

	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  ...
	  protected void Timer1_Tick(object sender, EventArgs e)
	  {
	     DataList1.DataBind();
	  }
	}
	````
		
	>**Note:** The timer periodically forces the page to refresh the contents of the guest book entries list.

1. Locate the **Page_Load** event handler and update its body with the following code to enable the page refresh timer.
	
	(Code Snippet – _Introduction to Windows Azure - Ex1 Page_Load_ – CS)
	
	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  ...
	  protected void Page_Load(object sender, EventArgs e)
	  {
	    if (!Page.IsPostBack)
	    {
	      Timer1.Enabled = true;
	    }
	  }
	}
	````
		
1. Implement the **InitializeStorage** method by replacing its body with the following (highlighted) code.

	(Code Snippet – _Introduction to Windows Azure - Ex1 InitializeStorage_ – CS)

	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  ...
	  private void InitializeStorage()
	  {
	    if (storageInitialized)
	    {
	      return;
	    }
	
	    lock (gate)
	    {
	      if (storageInitialized)
	      {
	        return;
	      }
	
	      try
	      {
	        // read account configuration settings
	        var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
	
	        // create blob container for images
	        blobStorage = storageAccount.CreateCloudBlobClient();
	        CloudBlobContainer container = blobStorage.GetContainerReference("guestbookpics");
	        container.CreateIfNotExist();
	
	        // configure container for public access
	        var permissions = container.GetPermissions();
	        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
	        container.SetPermissions(permissions);
	      }
	      catch (WebException)
	      {
	        throw new WebException("Storage services initialization failure. "
	           + "Check your storage account configuration settings. If running locally, "
	           + "ensure that the Development Storage service is running.");
	      }
	
	      storageInitialized = true;
	    }
	  }
	}
	````
			
	>**Note:** The **InitializeStorage** method first ensures that it executes only once. It reads the storage account settings from the Web role configuration, creates a blob container for the images uploaded with each guest book entry and configures it for public access.

1. Because the web role uses Windows Azure storage services, you need to provide your storage account settings. To create a new setting, in **Solution Explorer**, expand the **Roles** node in the **GuestBook** project, double-click **GuestBook_WebRole** to open the properties for this role and select the **Settings** tab. Click **Add Setting**, type _"DataConnectionString"_ in the **Name** column, change the **Type** to _Connection String_, and then click the button labeled with an ellipsis.

	![Configuring the storage account settings](images/configuring-storage-settings.png?raw=true "Configuring the storage account settings")

	_Configuring the storage account settings_

1. In the **Storage Account Connection String** dialog, choose the option labeled **Use the Windows Azure storage emulator** and then click **OK**. 

	![Creating a connection string for the storage emulator](images/connection-string-storage-emulator.png?raw=true "Creating a connection string for the storage emulator")

	_Creating a connection string for the storage emulator_ 

	>**Note:** A storage account is a unique endpoint for the Windows Azure Blob, Queue, and Table services. You must create a storage account in the Management Portal to use these services. In this exercise, you use Windows Azure storage emulator, which is included in the Windows Azure SDK development environment to simulate the Blob, Queue, and Table services available in the cloud. If you are building a hosted service that employs storage services or writing any external application that calls storage services, you can test locally against the Windows Azure storage emulator.

	>To use the storage emulator, you set the value of the **UseDevelopmentStorage** keyword in the connection string for the storage account to true. When you publish your application to Windows Azure, you need to update the connection string to specify storage account settings including your account name and shared key. For example,
	
	>\<Setting name="DataConnectionString" value="DefaultEndpointsProtocol=https;AccountName=YourAccountName;AccountKey=YourAccountKey" /\>
	
	>where _YourAccountName_ is the name of your Azure Storage account and YourAccountKey is your access key.

1. Press **CTRL + S** to save changes to the role configuration.

1. Finally, you need to set up the environment for the configuration publisher. In the **GuestBook_WebRole** project, open the **Global.asax.cs** file.

1. At the top of the file, insert the following namespace declaration to import the types contained in the Microsoft.**WindowsAzure** and **Microsoft.WindowsAzure.ServiceRuntime** namespaces.

	````C#
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.ServiceRuntime;
	````
		
1. Insert the following code into the **Application_Start** method replacing the default comment.

	(Code Snippet – _Introduction to Windows Azure - Ex1 SetConfigurationSettingPublisher_ – CS)

	````C#
	void Application_Start(object sender, EventArgs e)
	{
	    Microsoft.WindowsAzure.CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
	    {
	      configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
	    });
	}
	````

<a name="Ex1Task4" />
#### Task 4 – Queuing Work Items for Background Processing ####

In preparation for the next exercise, you now update the front-end web role to dispatch work items to an Azure queue for background processing. These work items will remain in the queue until you add a worker role that picks items from the queue and generates thumbnails for each uploaded image.

1. Open the code-behind file for the main page of the web role project. To do this, right-click the **Default.aspx** file in **Solution Explorer** and select **View Code**.

1. Declare a queue client member by inserting the following (highlighted) declaration into the **Default** class.

	(Code Snippet – _Introduction to Windows Azure - Ex1 CloudQueueClient member_ – CS)

	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  private static bool storageInitialized = false;
	  private static object gate = new Object();
	  private static CloudBlobClient blobStorage;
	  private static CloudQueueClient queueStorage;
	  ...
	}
	````
		
1. Now, update the storage initialization code to create the queue, if it does not exist, and then initialize the queue reference created in the previous step. To do this, locate the **InitializeStorage** method and insert the following (highlighted) code into this method immediately after the code that configures the blob container for public access.

	(Code Snippet – _Introduction to Windows Azure - Ex1 Create Queue_ – CS)

	````C#
	public partial class _Default : System.Web.UI.Page
	{
	  ...
	  private void InitializeStorage()
	  {
	    ...
	      try
	      {
	        ...
	        // configure container for public access
	        var permissions = container.GetPermissions();
	        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
	        container.SetPermissions(permissions);
	
	        // create queue to communicate with worker role
	        queueStorage = storageAccount.CreateCloudQueueClient();
	        CloudQueue queue = queueStorage.GetQueueReference("guestthumbs");
	        queue.CreateIfNotExist();
	      }
	      catch (WebException)
	      {
	        ...
	}
	````
		
	>**Note:** The updated code creates a queue that the web role uses to submit new jobs to the worker role.

1. Finally, add code to post a work item to the queue. To do this, locate the **SignButton_Click** event handler and insert the following (highlighted) code immediately after the lines that create a new entry in table storage.

    (Code Snippet – _Introduction to Windows Azure - Ex1  Queueing work items_ – CS)

	````C#
	protected void SignButton_Click(object sender, EventArgs e)
	{
	  if (FileUpload1.HasFile)
	  {
	    ...	
	    // create a new entry in table storage
	    GuestBookEntry entry = new GuestBookEntry() { GuestName = NameTextBox.Text, Message = MessageTextBox.Text, PhotoUrl = blob.Uri.ToString(), ThumbnailUrl = blob.Uri.ToString() };
	    GuestBookDataSource ds = new GuestBookDataSource();
	    ds.AddGuestBookEntry(entry);
	    System.Diagnostics.Trace.TraceInformation("Added entry {0}-{1} in table storage for guest '{2}'", entry.PartitionKey, entry.RowKey, entry.GuestName);
	
	    // queue a message to process the image
	    var queue = queueStorage.GetQueueReference("guestthumbs");
	    var message = new CloudQueueMessage(String.Format("{0},{1},{2}", blob.Uri.ToString(), entry.PartitionKey, entry.RowKey));
	    queue.AddMessage(message);
	    System.Diagnostics.Trace.TraceInformation("Queued message to process blob '{0}'", uniqueBlobName);
	  }
	
	  NameTextBox.Text = "";
	  MessageTextBox.Text = "";
	
	  DataList1.DataBind();
	}
	````
			
	>**Note:** The updated code obtains a reference to the _“guestthumbs”_ queue. It constructs a new message that consists of a comma-separated string with the name of the blob that contains the image, the partition key, and the row key of the entity that was added. The worker role can easily parse messages with this format. The method then submits the message to the queue.

<a name="Ex1Verification" />
#### Verification ####

The Windows Azure compute emulator, formerly Development Fabric or devfabric, is a simulated environment for developing and testing Windows Azure applications in your machine. In this task, you launch the GuestBook application in the emulator and create one or more guest book entries.

Among the features available in the Windows Azure Tools for Microsoft Visual Studio is a Windows Azure Storage browser that allows you to connect to a storage account and browse the blobs and tables it contains. If you are using this version of Visual Studio, you will use it during this task to examine the storage resources created by the application.

1. Press **F5** to execute the service. The service builds and then launches the local Windows Azure compute emulator. To show the Compute Emulator UI, right-click its icon located in the system tray and select **Show Compute Emulator UI**.

	![Showing the Compute Emulator UI](images/showing-the-compute-emulator-ui.png?raw=true "Showing the Compute Emulator UI")
 
	_Showing the Compute Emulator UI_
 
	>**Note:** If it is the first time you run the **Windows Azure Emulator**, the System will show a **Windows Security Alert** dialog indicating the Firewall has blocked some features. Click **Allow Access** to continue.
	
	>![Unblocking the Firewall](images/warning-unblocking-firewall.png?raw=true "Unblocking the Firewall")
	
	>**Note:** When you use the storage emulator for the first time, it needs to execute a one-time initialization procedure to create the necessary database and tables. If this is the case, wait for the procedure to complete and examine the **Development Storage Initialization** dialog to ensure that it completes successfully.

	![Storage emulator initialization process](images/initialization-process.png?raw=true "Storage emulator initialization process")

	_Storage emulator initialization process_

1. Switch to Internet Explorer to view the **GuestBook** application.

1. Add a new entry to the guest book. To do this, type your name and a message, choose an image to upload from the **Pictures\Sample Pictures** library, and then click the pencil icon to submit the entry.

	![Windows Azure GuestBook home page](images/guestbook-home-page.png?raw=true "Windows Azure GuestBook home page")

	_Windows Azure GuestBook home page_

	>**Note:** It is a good idea to choose a large hi-resolution image because, once the application is complete, the guestbook service will resize uploaded images.

	Once you submit an entry, the web role creates a new entity in the guest book table and uploads the photo to blob storage. The page contains a timer that triggers a page refresh every 5 seconds, so the new entry should appear on the page after a brief interval. 
	Initially, the new entry contains a link to the blob that contains the uploaded image so it will appear with the same size as the original image. 

	![GuestBook application showing an uploaded image in its original size](images/guestbook-application-uploaded-image.png?raw=true "GuestBook application showing an uploaded image in its original size")

	_GuestBook application showing an uploaded image in its original size_

	>**Note:** If you are using Visual Studio 2010, you can use the Windows Azure Storage Explorer to view storage resources directly from Visual Studio. This functionality is not available in Visual Studio 2008.

1. To open the Storage Explorer in Visual Studio 2010, open the **View** menu, select **Server Explorer**, and then expand the **Windows Azure Storage node**. The **Windows Azure Storage** node lists the storage accounts that you have currently registered and, by default, includes an entry for the storage emulator account labeled as **(Development)**.

1. Expand the **(Development)** node and then the **Tables** node inside it. Notice that it contains a table named _GuestBookEntry_ created by the application that should contain details for each entry.

	![Viewing tables in the Windows Azure storage emulator](images/unfolding-storage-emulator-tables.png?raw=true "Viewing tables in the Windows Azure storage emulator")

	_Viewing tables in the Windows Azure storage emulator_

1. Double-click the _GuestBookEntry_ node in the **Windows Azure Storage** explorer to show the contents of this table. The _GuestBookEntry_ table contains information for the entry that you created earlier in this task, including its _GuestName_, _Message_, _PhotoUrl_, and _ThumbnailUrl_ properties, as well as the _PartitionKey_, _RowKey_, and _Timestamp_ properties common to all table storage entities. Notice that the _PhotoUrl_ and _ThumbnailUrl_ properties are currently the same. In the next exercise, you will modify the application to generate image thumbnails and to update the corresponding URL.


	![Viewing tables in the Windows Azure storage emulator](images/storage-emulator-tables.png?raw=true "Viewing tables in the Windows Azure storage emulator")

	_Viewing tables in the Windows Azure storage emulator_

1. Now, expand the **Blobs** node in the **Windows Azure Storage** explorer. Inside this node, you will find an entry for a container named _guestbookpics_ that contains blobs with raw data for the images uploaded by the application. 

	![Viewing blobs using the Windows Azure Tools for Visual Studio](images/viewing-blobs.png?raw=true "Viewing blobs using the Windows Azure Tools for Visual Studio")

	_Viewing blobs using the Windows Azure Tools for Visual Studio_

1. Double-click the node for the _guestbookpics_ container to list the blobs it contains. It should include an entry for the image that you uploaded earlier

	![Viewing the contents of a blob container in Visual Studio](images/viewing-blob-contents.png?raw=true "Viewing the contents of a blob container in Visual Studio")

	_Viewing the contents of a blob container in Visual Studio_

1. Each blob in blob storage has an associated content type that Visual Studio uses to select a suitable viewer for the blob. To display the contents of the blob, double-click the corresponding entry in the container listing to display the image.

	![Viewing blob contents in Visual Studio](images/viewing-blob-contents-2.png?raw=true "Viewing blob contents in Visual Studio")

	_Viewing blob contents in Visual Studio_

1. Press **SHIFT + F5** to stop the debugger and shut down the deployment in the development fabric.

<a name="Exercise2" />
### Exercise 2: Background Processing with Worker Roles and Queues ###

A worker role runs in the background to provide services or execute time related tasks like a service process. 
In this exercise, you create a worker role to read work items posted to a queue by the web role front-end. To process the work item, the worker role extracts information about a guest book entry from the message and then retrieves the corresponding entity from table storage. It then fetches the associated image from blob storage and creates its thumbnail, which it also stores as a blob. Finally, to complete the processing, it updates the URL of the generated thumbnail blob in the guest book entry.

<a name="Ex2Task1" />
#### Task 1 – Creating a Worker Role to Process Images in the Background ####

In this task, you add a worker role project to the solution and update it so that it reads items posted by the front-end from the queue and processes them.

1. If not already open, launch Visual Studio as administrator from **Start | All Programs | Microsoft Visual Studio 2010** by right clicking the **Microsoft Visual Studio 2010** shortcut and choosing **Run as administrator**. 

1. In the **File** menu, choose **Open** and then **Project/Solution**. In the **Open Project** dialog, browse to **\source\Ex2-UsingWorkerRolesAndQueues\Begin**, select **Begin.sln** and click **Open**. Alternatively, you may continue with the solution that you obtained after completing the previous exercise.

1. In **Solution Explorer**, right-click the Roles node in the **GuestBook** project, point to **Add** and then select **New Worker Role Project**.

1. In the **Add New Role Project** dialog, select the **Worker Role** category and choose the **Worker Role** template. Set the name of the worker role to **GuestBook_WorkerRole** and click **Add**.

	![Adding a worker role project to the solution](images/adding-worker-role.png?raw=true "Adding a worker role project to the solution")

	_Adding a worker role project to the solution_

1. In the new worker role project, add a reference to the data model project. In **Solution Explorer**, right-click the **GuestBook_WorkerRole** project and select **Add Reference**, switch to the **Projects** tab, select **GuestBook_Data** and then click **OK**.

1. Next, add a reference to the **System.Drawing** assembly, only this time, in the Add Reference dialog, switch to the **.NET** tab instead, select the **System.Drawing** component and then click **OK**.

1. Now, open the **WorkerRole.cs** file of the **GuestBook_WorkerRole** project and insert the followings namespace declarations.

	(Code Snippet – _Introduction to Windows Azure - Ex2 WorkerRole Namespaces_ – CS)

	````C#
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.IO;
	using GuestBook_Data;
	````
	
1. Add member fields to the **WorkerRole** class for the blob container and the queue, as shown below.

	(Code Snippet – _Introduction to Windows Azure - Ex2 WorkerRole Fields_ – CS)

	````C#
	public class WorkerRole : RoleEntryPoint
	{
	   private CloudQueue queue;
	   private CloudBlobContainer container;
	  ...
	}
	````
		
1. Insert the following code into the body of the **OnStart** method immediately after the line that subscribes the **RoleEnvironmentChanging** event and before the call to the **OnStart** method in the base class.

	(Code Snippet – _Introduction to Windows Azure - Ex2 WorkerRole OnStart_ – CS)

	````C#
	public class WorkerRole : RoleEntryPoint
	{
	  ...
	  public override bool OnStart()
	  {
	    // Set the maximum number of concurrent connections 
	    ServicePointManager.DefaultConnectionLimit = 12;
	
	    // read storage account configuration settings
	    CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
	    {
	      configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
	    });
	    var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
	
	    // initialize blob storage
	    CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
	    container = blobStorage.GetContainerReference("guestbookpics");
	
	    // initialize queue storage 
	    CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();
	    queue = queueStorage.GetQueueReference("guestthumbs");
	
	    Trace.TraceInformation("Creating container and queue...");
	
	    bool storageInitialized = false;
	    while (!storageInitialized)
	    {
	       try
	       {
	          // create the blob container and allow public access
	          container.CreateIfNotExist();
	          var permissions = container.GetPermissions();
	          permissions.PublicAccess = BlobContainerPublicAccessType.Container;
	          container.SetPermissions(permissions);
	
	          // create the message queue(s)
	          queue.CreateIfNotExist();
	
	          storageInitialized = true;
	      }
	      catch (StorageClientException e)
	      {
	         if (e.ErrorCode == StorageErrorCode.TransportError)
	         {
	              Trace.TraceError("Storage services initialization failure. "
	              + "Check your storage account configuration settings. If running locally, "
	              + "ensure that the Development Storage service is running. Message: '{0}'", e.Message);
	              System.Threading.Thread.Sleep(5000);
	            }
	            else
	            {
	               throw;
	            }
	         }
	       }
	
	       return base.OnStart();
	     }
	  ...
	}
	````
	
1. Replace the body of the **Run** method with the code shown below.

	(Code Snippet – _Introduction to Windows Azure - Ex2 WorkerRole Run_ – CS)

	````C#
	public class WorkerRole : RoleEntryPoint
	{
	  ...
	  public override void Run()
	  {
	    Trace.TraceInformation("Listening for queue messages...");
	
	    while (true)
	    {
	      try
	      {
	        // retrieve a new message from the queue
	        CloudQueueMessage msg = queue.GetMessage();
	        if (msg != null)
	        {
	          // parse message retrieved from queue
	          var messageParts = msg.AsString.Split(new char[] { ',' });
	          var imageBlobUri = messageParts[0];
	          var partitionKey = messageParts[1];
	          var rowkey = messageParts[2];
	          Trace.TraceInformation("Processing image in blob '{0}'.", imageBlobUri);
	
	          string thumbnailBlobUri = System.Text.RegularExpressions.Regex.Replace(imageBlobUri, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2");
	
	          CloudBlob inputBlob = container.GetBlobReference(imageBlobUri);
	          CloudBlob outputBlob = container.GetBlobReference(thumbnailBlobUri);
	
	          using (BlobStream input = inputBlob.OpenRead())
	          using (BlobStream output = outputBlob.OpenWrite())
	          {
	            ProcessImage(input, output);
	
	            // commit the blob and set its properties
	            output.Commit();
	            outputBlob.Properties.ContentType = "image/jpeg";
	            outputBlob.SetProperties();
	
	            // update the entry in table storage to point to the thumbnail
	            GuestBookDataSource ds = new GuestBookDataSource();
	            ds.UpdateImageThumbnail(partitionKey, rowkey, thumbnailBlobUri);
	
	            // remove message from queue
	            queue.DeleteMessage(msg);
	
	            Trace.TraceInformation("Generated thumbnail in blob '{0}'.", thumbnailBlobUri);
	          }
	        }
	        else
	        {
	          System.Threading.Thread.Sleep(1000);
	        }
	      }
	      catch (StorageClientException e)
	      {
	        Trace.TraceError("Exception when processing queue item. Message: '{0}'", e.Message);
	        System.Threading.Thread.Sleep(5000);
	      }
	    }
	  }
	  ...
	}
	````
	
1. Finally, add the following method to the **WorkerRole** class to create thumbnails from a given image.

	(Code Snippet – _Introduction to Windows Azure - Ex2 ProcessImage_ – CS)

	````C#
	public class WorkerRole : RoleEntryPoint
	{
	  ...
	  public void ProcessImage(Stream input, Stream output)
	  {
	    int width;
	    int height;
	    var originalImage = new Bitmap(input);
	
	    if (originalImage.Width > originalImage.Height)
	    {
	      width = 128;
	      height = 128 * originalImage.Height / originalImage.Width;
	    }
	    else
	    {
	      height = 128;
	      width = 128 * originalImage.Width / originalImage.Height;
	    }
	
	    var thumbnailImage = new Bitmap(width, height);
	
	    using (Graphics graphics = Graphics.FromImage(thumbnailImage))
	    {
	      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
	      graphics.SmoothingMode = SmoothingMode.AntiAlias;
	      graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
	      graphics.DrawImage(originalImage, 0, 0, width, height);
	    }
	
	    thumbnailImage.Save(output, ImageFormat.Jpeg);
	  }
	}
	````
		
	>**Note:** Even though the code shown above uses classes in the System.Drawing namespace for simplicity, you should be aware that the classes in this namespace were designed for use with Windows Forms. They are not supported for use within a Windows or ASP.NET service. You should conduct exhaustive testing if you intend to use these classes in your own Windows Azure applications.

1. The worker role also uses Windows Azure storage services and you need to configure your storage account settings, just as you did in the case of the web role. To create the storage account setting, in **Solution Explorer**, expand the **Roles** node of the **GuestBook** project, double-click **GuestBook_WorkerRole** to open the properties for this role and select the **Settings** tab. Click **Add Setting**, type _“DataConnectionString”_ in the **Name** column, change the **Type** to _Connection String_, and then click the button labeled with an ellipsis. In the **Storage Account Connection String** dialog, choose the option labeled **Use the Windows Azure storage emulator** and click **OK**. Press **CTRL + S** to save your changes.

<a name="Ex2Verification" />
#### Verification ####

You now launch the updated application in the Windows Azure compute emulator to verify that the worker role can retrieve queued work items and generate the corresponding thumbnails.

1. Press **F5** to launch the service in the local compute emulator. 

1. Switch to Internet Explorer to view the application. If you completed the verification section of the previous exercise successfully, you will see the guest book entry that you entered, including the uploaded image displayed in its original size. If you recall, during the last task of that exercise, you updated the web role code to post a work item to a queue for each new entry submitted. These messages remain in the queue even though the web role was subsequently recycled. 

1. Wait a few seconds until the worker role picks up the queued message and processes the image that you are viewing. Once that occurs, it generates a thumbnail for this image and updates the corresponding URL property for the entry in table storage. Eventually, because the page refreshes every few seconds, it will show the thumbnail image instead.

	![Home page showing the thumbnail generated by the worker role](images/guestbook-thumbnail.png?raw=true "Home page showing the thumbnail generated by the worker role")

	_Home page showing the thumbnail generated by the worker role_

1. If you are using Visual Studio 2010, in **Server Explorer**, expand the **Blobs** node in the **Windows Azure Storage** node, and then double-click the _guestbookpics_ container. Notice that it now contains an additional blob for the generated thumbnail image.

	![Blob container showing the blob for the generated thumbnail](images/thumbnail-blob.png?raw=true "Blob container showing the blob for the generated thumbnail")

	_Blob container showing the blob for the generated thumbnail_

1. Add some more guest book entries. Notice that the images update after a few seconds once the worker role processes the thumbnails.

1. Press **SHIFT + F5** to stop the debugger and shut down the deployment in the compute emulator.

<a name="Exercise3" />
### Exercise 3: Publishing a Windows Azure Application ###

In this exercise, you publish the application created in the previous exercise to Windows Azure using the Management Portal. First, you provision the required service components, upload the application package to the staging area and configure it. You then execute the application in the staging area to verify its operation. Finally, you promote the application to production.

>**Note:** In order to complete this exercise, you need to sign up for a Windows Azure account and purchase a subscription. 
>For a description of the provisioning process, see [Provisioning Windows Azure](http://blogs.msdn.com/david_sayed/archive/2010/01/07/provisioning-windows-azure.aspx).

<a name="Ex3Task1" />
#### Task 1 – Creating a Storage Account and a Hosted Service Component ####

The application you publish in this exercise requires both compute and storage services. In this task, you create a new Windows Azure storage account to allow the application to persist its data. In addition, you define a hosted service component to execute application code.

1. Navigate to [http://windows.azure.com](http://windows.azure.com) using a Web browser and sign in using the Windows Live ID associated with your Windows Azure account.

	![Signing in to the Windows Azure Management Portal](images/sign-in.png?raw=true "Signing in to the Windows Azure Management Portal")

	_Signing in to the Windows Azure Management Portal_

1. First, you create the storage account that the application will use to store its data. In the Windows Azure ribbon, click **New Storage Account**.

	![Creating a new storage account](images/new-storage-account.png?raw=true "Creating a new storage account")

	_Creating a new storage account_

1. In the **Create a New Storage Account** dialog, pick your subscription in the drop down list labeled **Choose a subscription**.

	![Choosing a subscription to host the storage account](images/choose-subscription-account.png?raw=true "Choosing a subscription to host the storage account")

	_Choosing a subscription to host the storage account_

1. In the textbox labeled **Enter a URL**, enter the name for your storage account, for example, **\<yourname\>guestbook**, where \<yourname\> is a unique name. Windows Azure uses this value to generate the endpoint URLs for the storage account services.

	![Choosing the URL of the new storage account](images/choosing-affinity-group.png?raw=true "Choosing the URL of the new storage account")

	_Choosing the URL of the new storage account_

	>**Note:** The portal ensures that the name is valid by verifying that the name complies with the naming rules and is currently available. A validation error will be shown if you enter a name that does not satisfy the rules.

	>![Name verification](images/warning-name-verification.png?raw=true "Name verification")

1. Select the option labeled **Create or choose an affinity group** and then pick **Create a new affinity group** from the drop down list.

	![Creating a new affinity group](images/new-affinity-group.png?raw=true "Creating a new affinity group")

	_Creating a new affinity group_

	>**Note:** The reason that you are creating a new affinity group is to deploy both the hosted service and storage account to the same location, thus ensuring high bandwidth and low latency between the application and the data it depends on.

1. In the **Create a New Affinity Group** dialog, enter an **Affinity Group Name**, select its **Location** in the drop down list, and then click **OK**.

	![Creating a new affinity group](images/new-affinity-group-2.png?raw=true "Creating a new affinity group")

	_Creating a new affinity group_

1. Back in the **Create a New Storage Account** dialog, click **OK** to register your new storage account. Wait until the account provisioning process completes and updates the **Storage Accounts** tree view. Notice that the **Properties** pane shows the **URL** assigned to each service in the storage account. Record the public storage account name—this is the first segment of the URL assigned to your endpoints. 

	![Storage account successfully created](images/storage-account-created.png?raw=true "Storage account successfully created")

	_Storage account successfully created_

1. Now, click the **View** button next to **Primary access key** in the **Properties** pane. In the **View Storage Access Keys** dialog, click **Copy to Clipboard** next to the **Primary Access Key**. You will use this value later on to configure the application.

	![Retrieving the storage access keys](images/retrieving-access-keys.png?raw=true "Retrieving the storage access keys")

	_Retrieving the storage access keys_

	>**Note:** The **Primary Access Key** and **Secondary Access Key** both provide a shared secret that you can use to access storage. The secondary key gives the same access as the primary key and is used for backup purposes. You can regenerate each key independently in case either one is compromised.

1. Next, create the compute component that executes the application code. Click **Hosted Services** on the left pane. Click on **New Hosted Service** button on the ribbon. 

	![Creating a new hosted service](images/new-hosted-service.png?raw=true "Creating a new hosted service")

	_Creating a new hosted service_

1. In the **Create a new Hosted Service** dialog, select the subscription where you wish to create the service from the drop down list labeled **Choose a subscription**.

	![Choosing your subscription](images/choosing-your-subscription.png?raw=true "Choosing your subscription")

	_Choosing your subscription_

1. Enter a service name in the textbox labeled **Enter a name for your service** and choose its URL  by entering a prefix in the textbox labeled **Enter a URL prefix for your service**, for example, **\<yourname\>guestbook**, where <_yourname_> is a unique name. Windows Azure uses this value to generate the endpoint URLs for the hosted service.

	![Choosing a service name and URL](images/configuring-hosted-service-name-url.png?raw=true "Choosing a service name and URL")

	_Choosing a service name and URL_

	>**Note:** If possible, choose the same name for both the storage account and hosted service. However, you may need to choose a different name if the one you select is unavailable.

	>The portal ensures that the name is valid by verifying that the name complies with the naming rules and is currently available. A validation error will be shown if you enter name that does not satisfy the rules.
	
	>![Warning message: URL already taken](images/warning-taken-url.png?raw=true "Warning message: URL already taken")

1. Select the option labeled **Create or choose an affinity group** and then pick the **guestbook** affinity group from the drop down list—this is the same affinity group that you defined earlier, when you created the storage account.

	![Choosing an affinity group](images/choosing-guestbook-affinity-group.png?raw=true "Choosing an affinity group")

	_Choosing an affinity group_

	>**Note:** By choosing **guestbook** as the affinity group, you ensure that the hosted service is deployed to the same location as the storage account that you provisioned earlier.

1. Select the option labeled **Do not Deploy**.

	>**Note:** While you can create and deploy your service to Windows Azure in a single operation by completing the **Deployment Options** section, for this hands-on lab, you will defer the deployment step until the next task.

1. Click **OK** to create the hosted service and then wait until the provisioning process completes.

	![Hosted service successfully created](images/hosted-service-created.png?raw=true "Hosted service successfully created")

	_Hosted service successfully created_

1. Do not close the browser window. You will use the portal for the next task.

<a name="Ex3Task2" />
#### Task 2 – Publishing the Application to the Windows Azure Management Portal ####

There are several alternatives for publishing applications to Windows Azure. The Windows Azure Tools for Visual Studio allow you to both create and publish the service package to the Windows Azure environment directly from Visual Studio. Another deployment option is the [Windows Azure Service Management PowerShell Cmdlets](http://wappowershell.codeplex.com/) that enable a scripted deployment of your application. Lastly, the Windows Azure Management Portal provides the means to publish and manage your service using only your browser. For more information about publishing applications, see the **Windows Azure Deployment** lab in this training kit.

In this task, you publish the application to the staging environment using the Management Portal but first, you generate the service package using Visual Studio.

1. If it is not already open, launch Visual Studio as administrator from **Start | All Programs | Microsoft Visual Studio 2010** by right clicking the **Microsoft Visual Studio 2010** shortcut and choosing **Run as administrator**. 

1. If the **User Account Control** dialog appears, click **Continue**.

1. In the **File** menu, choose **Open** and then **Project/Solution**. In the **Open Project** dialog, browse to **\source\Ex3-WindowsAzureDeployment**. Select **Begin.sln** in the **Begin** folder and click **Open**. 
Alternatively, you may continue with the solution that you obtained after completing the previous exercise. 

1. To configure the storage before publishing the service, open the **ServiceConfiguration.cscfg** file you will use to publish the project (e.g. **ServiceConfiguration.Cloud.cscfg**), which is located in **GuestBook** service. Replace the placeholder labeled _[YOUR\_ACCOUNT\_NAME]_ with the **Storage Account Name** that you chose when you configured the storage account in Task 1. If you followed the recommendation, the name should follow the pattern ***\<yourname\>guestbook***, where <_yourname_> is a unique name. Make sure to replace both instances of the placeholder, one for the _DataConnectionString_ and the second one for the _Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString_.

	>**Note:** If you continued working with the solution obtained in the previous exercise, you need to change the **DataConnectionString** and **Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString** values.
	Replace their value with the following:
	>
	> DefaultEndpointsProtocol=https;AccountName=[YOUR_ACCOUNT_NAME];AccountKey=[YOUR_ACCOUNT_KEY]
	>	
	> Then, replace the placeholders with your **Storage Account Name** and **Primary Access Key**.

1. Next, replace the placeholder labeled _[YOUR_ACCOUNT_KEY]_ with the **Primary Access Key** value that you recorded earlier, when you created the storage account in Task 1. Again, replace both instances of the placeholder, one for each connection string.

	![Configuring the storage account connection string](images/configuring-storage-account-connection.png?raw=true "Configuring the storage account connection string")

	_Configuring the storage account connection strings_

1. Generate the package to publish to the cloud. To do this, right-click the **GuestBook** cloud project and select **Package**. In the **Package Windows Azure Application** dialog, select the **Service Configuration** and the **Build Configuration** you will use from the dropdowns and then click **Package**. 
After Visual Studio builds the project and generates the service package, Windows Explorer opens with the current folder set to the location where the generated package is stored.

	![Creating a service package in Visual Studio](images/creating-a-service-package.png?raw=true "Creating a service package in Visual Studio")
	
	_Creating a service package in Visual Studio_

	>**Note:** Although the procedure is not shown here, you can use the Publish Cloud Service feature in the Windows Azure Tools to publish your service package directly from Visual Studio. To use this feature, you need to configure a set of credentials that you use to authenticate access to the management service using a self-issued certificate that you upload to the Management Portal.

1. Now, switch back to the browser window and the Management Portal.

1. At the portal, select the hosted service that you created in the previous step and then click **New Staging Deployment** on the ribbon. 

	>**Note:** A hosted service is a service that runs your code in the Windows Azure environment. It has two separate deployment slots: staging and production. The staging deployment slot allows you to test your service in the Windows Azure environment before you deploy it to production.


	![Hosted service summary page](images/hosted-service-summary-page.png?raw=true "Hosted service summary page")

	_Hosted service summary page_

1. In the **Create a new Deployment** dialog, to select a **Package location**, click **Browse Locally**, navigate to the folder where Visual Studio generated the package in Step 4 and then select **GuestBook.cspkg**. 

1. Now, to choose the **Configuration File**, click **Browse Locally** and select **ServiceConfiguration.cscfg** in the same folder that you used in the previous step.

	>**Note:** The _.cscfg_ file contains configuration settings for the application, including the instance count that you will update later in the exercise.

1. Finally, for the **Deployment name**, enter a label to identify the deployment; for example, use **v1.0**.

	>**Note:** The portal displays the label in its user interface for staging and production, allowing you to identify the version currently deployed in each environment.

	![Configuring the service package deployment](images/configuring-service-package-deployment.png?raw=true "Configuring the service package deployment")

	_Configuring the service package deployment_

1. Click **OK** to start the deployment. Notice that the portal displays a warning message when you do this. Click **See more details** to review and understand the message. 

	![Reviewing the warnings](images/warnings-rev.png?raw=true "Reviewing the warnings")

	_Reviewing the warnings_

	>**Note**: In this particular case, the warning indicates that only a single instance is being deployed for at least one of the roles. This is not recommended because it does not guarantee the service’s availability. In the next task, you will increase the number of instances to overcome this issue.

	>![Single instance deploy warning](images/note-deployment-warning.png?raw=true "Single instance deploy warning")

1. Click **Yes** to override and submit the deployment request. Notice that the package begins to upload and that the portal shows the status of the deployment to indicate its progress. 

	![Uploading a service package to the Windows Azure Management Portal](images/uploading-service-package-waz.png?raw=true "Uploading a service package to the Windows Azure Management Portal")

	_Uploading a service package to the Windows Azure Management Portal_


1. Wait until the deployment process finishes, which may take several minutes. At this point, you have already uploaded the package and it is in a **Ready** state. Notice that the portal assigned a **DNS name** to the deployment that includes a unique identifier. Shortly, you will access this URL to test the application and determine whether it operates correctly in the Windows Azure environment, but first you need to configure it.
	
	>**Note:** During deployment, Windows Azure analyzes the configuration file and copies the service to the correct number of machines, and starts all the instances. Load balancers, network devices and monitoring are also configured during this time.

	![Package successfully deployed and ready](images/package-successfully-deployed.png?raw=true "Package successfully deployed and ready")

	_Package successfully deployed and ready_

<a name="Ex3Task3" />
#### Task 3 – Configuring the Application to Increase the Number of Instances ####

Before you can test the deployed application, you need to configure it. In this task, you define the storage account settings for the application.

1. In **Hosted Services**, select your **GuestBook** service and click **Configure** on the ribbon.


	![Configuring application settings](images/configuring-application-settings.png?raw=true "Configuring application settings")

	_Configuring application settings_

1. In the **Configure Deployment** dialog, select the option labeled **Edit current configuration**, locate the **Instances** element inside the **GuestBook_WebRole** configuration and change its **count** attribute to _"2"_. Do the same for the **GuestBook_WorkerRole** configuration to also increase its instance count to _"2"_.

	> **Note:** The configuration is simply an XML document that contains the value of the settings declared in the service definition file. Its initial content is determined by the **ServiceConfiguration.cscfg** file that you uploaded earlier, when you deployed the package in Task 2.

	> The **Instances** setting controls the number of roles that Windows Azure starts and is used to scale the service. For a token-based subscription—currently only available in countries that are not provisioned for billing—this number is limited to a maximum of two instances. However, in the commercial offering, you can change it to any number that you are willing to pay for.

1. Click **OK** to update the configuration and wait for the hosted service to apply the new settings. 

	![Configuring the Instances count](images/configuring-instances-count.png?raw=true "Configuring the Instances count")

	_Configuring the Instances count_

	>**Note:** The portal displays a legend _"Updating deployment..."_ while the settings are applied.

<a name="Ex3Task4" />
#### Task 4 – Testing the Application in the Staging Environment ####

In this task, you run the application in the staging environment and access its public endpoint to test that it operates correctly.

1. In **Hosted Services**, select your **GuestBook** service and then click the link located in the right pane under **DNS name**. 

	![Running the application in the staging environment](images/running-application-staging.png?raw=true "Running the application in the staging environment")

	_Running the application in the staging environment_

	>**Note:** The link shown for DNS name has the form _\<guid\>.cloudapp.net_, where <_guid_> is some random identifier. This is different from the address where the application will run once it is in production. Although the application executes in a staging area that is separate from the production environment, there is no actual physical difference between staging and production – it is simply a matter of where the load balancer is connected. 
	>	
	>![DNS Name](images/dns-name.png?raw=true "DNS Name")

1. If you wish, you may test the application by signing the guest book and uploading an image.

	![Application running in the staging environment](images/application-staging.png?raw=true "Application running in the staging environment")	

	_Application running in the staging environment_

<a name="Ex3Task5" />
#### Task 5 – Promoting the Application to Production ####

Now that you have verified that the service is working correctly in the staging environment, you are ready to promote it to final production. When you deploy the application to production, Windows Azure reconfigures its load balancers so that the application is available at its production URL.

1. In **Hosted Services**, select your **GuestBook** service and then click **Swap VIP** on the ribbon.

	![Promoting the application to the production slot](images/promoting-to-production.png?raw=true "Promoting the application to the production slot")
	

	_Promoting the application to the production slot_

1. On the **Swap VIPs** dialog, click **OK** to swap the deployments between staging and production.

	![Promoting the application to the production slot](images/promoting-to-production-dialog.png?raw=true "Promoting the application to the production slot")

	_Promoting the application to the production slot_

1. Wait for the promotion process to complete, which typically takes a few seconds.

	![Application successfully deployed to production](images/application-deployed-to-production.png?raw=true "Application successfully deployed to production")

	_Application successfully deployed to production_

1. Click the **DNS name** link to open the production site in a browser window and notice the URL in the address bar.

	![Application running in the production environment](images/application-production.png?raw=true "Application running in the production environment")

	_Application running in the production environment_

	>**Note:** If you visit the production site shortly after its promotion, the DNS name might not be ready. If you encounter a DNS error (404), wait a few minutes and try again. Keep in mind that Windows Azure creates DNS name entries dynamically and that the changes might take few minutes to propagate.

1. Even when a deployment is in a suspended state, Windows Azure still needs to allocate a virtual machine (VM) for each instance and charge you for it. Once you have completed testing the application, you need to remove the deployment from Windows Azure to avoid an unnecessary expense. To remove a running deployment, go to **Hosted Services**, select the deployment slot where the service is currently hosted, staging or production, and then click **Stop** on the ribbon. Once the service has stopped, click **Delete** on the ribbon to remove it.

---

## Summary ##

By completing this hands-on lab, you have reviewed the basic elements of Windows Azure applications. You have seen that services consist of one or more web roles and worker roles. You have learned about Windows Azure storage services and in particular, Blob, Table and Queue services. Finally, you have explored a basic architectural pattern for cloud applications that allows front-end processes to communicate with back-end processes using queues.