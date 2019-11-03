# EfCoreSqlAndCosmos

This example application goes with the following articles:
* An in-depth study of Cosmos DB and EF Core 3.0 database provider - waiting for link!
* [Building a robust CQRS database with EF Core and Cosmos DB](https://www.thereformedprogrammer.net/building-a-robust-cqrs-database-with-ef-core-and-cosmos-db/).

## How to build and run this application

You need:

1. NET Core 3.0 installed (installing Visual Studio 2019 should install NET Core 3.0).
2. You need SQL Service localhost, e.g.`(localdb)\\mssqllocaldb`, which is installed with Visual Studio 2019.
3. You need [Azure Cosmos DB Emulator for local development and testing](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) installed and running.


The first time you run the `EfCore3SqlAndCosmos` ASP.NET Core application it will create a SQL database called `EfCoreSqlAndCosmos-Sql` and a Cosmos DB database called `EfCoreSqlAndCosmos`. It will then proceed to seed them with 55 books.

