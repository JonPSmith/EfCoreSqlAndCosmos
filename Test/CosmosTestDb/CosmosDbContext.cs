// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Test.CosmosTestDb
{
    public class CosmosDbContext : DbContext
    {
        public DbSet<CosmosBook> Books { get; set; }

        public CosmosDbContext(DbContextOptions<CosmosDbContext> options)
            : base(options) { }
    }
}