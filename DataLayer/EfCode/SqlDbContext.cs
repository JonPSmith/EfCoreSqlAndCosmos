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
            if (_bookUpdater == null)
                //This handles the case where you don't want the automatic write to Cosmos, e.g. when doing bulk loading
                return base.SaveChanges(acceptAllChangesOnSuccess);

            try
            {
                var thereAreChanges = _bookUpdater.FindBookChangesToProjectToNoSql(this);
                //This stops ChangeTracker being called twice
                ChangeTracker.AutoDetectChangesEnabled = false; 
                if (!thereAreChanges)
                    return base.SaveChanges(acceptAllChangesOnSuccess);
                return _bookUpdater.CallBaseSaveChangesAndNoSqlWriteInTransaction(this,
                    () => base.SaveChanges(acceptAllChangesOnSuccess));
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_bookUpdater == null)
                //This handles the case where you don't want the automatic write to Cosmos, e.g. when doing bulk loading
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            try
            {
                var thereAreChanges = _bookUpdater.FindBookChangesToProjectToNoSql(this);
                //This stops ChangeTracker being called twice
                ChangeTracker.AutoDetectChangesEnabled = false; 
                if (!thereAreChanges)
                    return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                return await _bookUpdater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(this,
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

