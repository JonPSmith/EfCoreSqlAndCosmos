# EfCoreSqlAndCosmos

This example application goes with the article [Building a robust CQRS database with EF Core and Cosmos DB](https://www.thereformedprogrammer.net/building-a-robust-cqrs-database-with-ef-core-and-cosmos-db/).

## How to build and run this application

You need:

1. NET Core 2.2 installed.
2. You need SQL Service localhost, e.g.`(localdb)\\mssqllocaldb`, which is installed with Visual Studio 2017.
3. You need [Azure Cosmos DB Emulator for local development and testing](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) installed and running.
4. You need to update the authKey in the appsetting.json file to the Primary Key from the running Cosmos DB Emulator

The first time you run the `EfCoreSqlAndCosmos` ASP.NET Core application it will create a SQL database called `EfCoreSqlAndCosmos-Sql` and a Cosmos DB database called `LocalCosmosDb`. It will then proceed to seed them with 55 books.
