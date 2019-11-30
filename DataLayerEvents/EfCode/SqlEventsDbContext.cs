// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode.Configurations;
using GenericEventRunner.ForDbContext;
using Microsoft.EntityFrameworkCore;

namespace DataLayerEvents.EfCode
{
    public class SqlEventsDbContext : DbContextWithEvents<SqlEventsDbContext>
    {
        public SqlEventsDbContext(DbContextOptions<SqlEventsDbContext> options, IEventsRunner eventsRunner)      
            : base(options, eventsRunner)
        {
        }

        public DbSet<BookWithEvents> Books { get; set; }
        public DbSet<AuthorWithEvents> Authors { get; set; }
        public DbSet<BookAuthorWithEvents> BookAuthors { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookWithEventsConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorWithEventsConfig());
            modelBuilder.ApplyConfiguration(new AuthorWithEventsConfig());
        }
    }
}

