// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClassesWithEvents;
using DataLayer.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SqlEventsDbContext : DbContext
    {

        public SqlEventsDbContext(DbContextOptions<SqlEventsDbContext> options)      
            : base(options)
        {
        }

        public DbSet<BookWithEvents> Books { get; set; }
        public DbSet<AuthorWithEvents> Authors { get; set; }
        public DbSet<BookAuthorWithEvents> BookAuthors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookWithEventsConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorWithEventsConfig());
        }
    }
}

