// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClassesNoSql;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class NoSqlDbContext : DbContext
    {
        private readonly string _containerName;
        private readonly string _partitionKey;

        public DbSet<BookListNoSql> Books { get; set; }

        public NoSqlDbContext(DbContextOptions<NoSqlDbContext> options, 
            string containerName = nameof(NoSqlDbContext),
            string partitionKey = null)
            : base(options)
        {
            _containerName = containerName;
            _partitionKey = partitionKey;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Model.SetDefaultContainer(_containerName);
            if (_partitionKey != null)
                modelBuilder.Entity<BookListNoSql>().HasPartitionKey(_partitionKey);
        }
    }
}