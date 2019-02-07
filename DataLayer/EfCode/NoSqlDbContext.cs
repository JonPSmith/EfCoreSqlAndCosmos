// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClassesNoSql;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class NoSqlDbContext : DbContext
    { 
        public NoSqlDbContext(DbContextOptions<NoSqlDbContext> options)
            : base(options) { }

        public DbSet<BookListNoSql> Books { get; set; }

        //thanks to https://csharp.christiannagel.com/2018/09/05/efcorecosmos/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //see this for timestamp https://docs.microsoft.com/en-us/azure/cosmos-db/working-with-dates#storing-datetimes
            modelBuilder.Entity<BookListNoSql>().Property<long>("_ts").ValueGeneratedOnAddOrUpdate();


        }
    }
}