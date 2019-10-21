// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.EfClassesSql;
using DataLayer.EfCode.Configurations;
using DataLayer.NoSqlCode;
using DataLayer.SqlCode;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SqlDbContext : DbContext
    {
        private readonly IBookUpdater _bookUpdater;

        //NOTE: if the bookUpdater isn't provided, then it reverts to a normal SaveChanges.
        public SqlDbContext(DbContextOptions<SqlDbContext> options, IBookUpdater bookUpdater = null)      
            : base(options)
        {
            _bookUpdater = bookUpdater;
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }

        //I only have to override these two version of SaveChanges, as the other two versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {

            try
            {
                var numBooksChanged = _bookUpdater?.FindNumBooksChanged(this) ?? 0;
                //This stops ChangeTracker being called twice
                ChangeTracker.AutoDetectChangesEnabled = false; 
                if (numBooksChanged == 0)
                    return base.SaveChanges(acceptAllChangesOnSuccess);
                return _bookUpdater.CallBaseSaveChangesAndNoSqlWriteInTransaction(this, numBooksChanged,
                    () => base.SaveChanges(acceptAllChangesOnSuccess));
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var numBooksChanged = _bookUpdater?.FindNumBooksChanged(this) ?? 0;
                //This stops ChangeTracker being called twice
                ChangeTracker.AutoDetectChangesEnabled = false; 
                if (numBooksChanged == 0)
                    return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                return await _bookUpdater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(this, numBooksChanged,
                    () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig());

            modelBuilder.HasDbFunction(() => UdfDefinitions.AverageVotesUdf(default(Guid)));
            modelBuilder.HasDbFunction(() => UdfDefinitions.AuthorsStringUdf(default(Guid)));
        }
    }
}

