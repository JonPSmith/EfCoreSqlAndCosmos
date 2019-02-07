// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClassesSql;
using DataLayer.EfCode.Configurations;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SqlDbContext : DbContext
    {
        private IBookUpdater _bookUpdater;

        public SqlDbContext(DbContextOptions<SqlDbContext> options, IBookUpdater bookUpdater = null)      
            : base(options)
        {
            _bookUpdater = bookUpdater;
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
        }
    }
}

