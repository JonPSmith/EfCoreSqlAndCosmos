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

    }
}