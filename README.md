# EfCoreSqlAndCosmos

This example application provides three different ways of showing data. They are:

* **Books (SQL):** This is uses a good, but not performanced-tuned EF Core query.
* **Books, event updates:** This uses cached values for the author's names and the review values to provide superior performance.
* **Books (NoSQL):** This uses a two-database CQRS design, with NoSQL (Cosmos Db) read-side database to provide superior performance and scalability.

The first time you run the ASP.NET Core application it will create a database(s) if its not there and seed the database with 55 books so you can see it in action. There is also an Admin->Generate feature which can generate a large number of books for performance tests.

## How to build and run this application

You need:

1. NET Core 3.1 installed (installing Visual Studio 2019.4 should install NET Core 3.1 for you).
2. You need SQL Service localhost, e.g.`(localdb)\\mssqllocaldb`, which is installed with Visual Studio 2019.
3. If you have enabled the NoSQL part (see below) you need [Azure Cosmos DB Emulator for local development and testing](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) installed and running.

### Controlling if the NoSQL database is used

To make it easier for people wanting to try this application, by default I turn off the NoSQL part. That's because the NoSQL part will fail on startup if the Azure Cosmos DB Emulator is not running locally.

Whether the NoSQL part is on or off is controlled by the `StartupMode` property in the appsettings.json file. I set this to "SqlOnly", which turns off the NoSQL part. 

If you want to try the NoSQL part then you should set the `StartupMode` to "SqlAndCosmosDb" and restart the application. **NOTE: if you previously started in "SqlOnly" you need to use Admin->Reset Database or Admin->Generate Books with wipe database ticked to rebuild the two database in sync.**


## Articles that go with this application

### Books, SQL

I haven't got an specific article on this, but I covered it in my book [Entity Framework Core in Action](http://bit.ly/2m8KRAZ).

### Books, event updates:

The article called [A technique for building high-performance databases with EF Core](#)  describes how I build this version, with its cached values.

### Books (NoSQL):

* [Building a robust CQRS database with EF Core and Cosmos DB](https://www.thereformedprogrammer.net/building-a-robust-cqrs-database-with-ef-core-and-cosmos-db/).  
This explains how the two-database CQRS works (its written with old EF Core 2.2 Cosmos provider, but the ideas are the same as in the real thing - see next article.).
* [An in-depth study of Cosmos DB and EF Core 3.0 database provider](https://www.thereformedprogrammer.net/an-in-depth-study-of-cosmos-db-and-ef-core-3-0-database-provider/) 
This looks at the issues I had when when I upgraded to the first, proper EF Core Cosmos database provider in EF Core 3.0.



