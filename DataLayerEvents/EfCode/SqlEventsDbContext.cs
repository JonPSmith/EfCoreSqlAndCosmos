// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayerEvents.DomainEventCode;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayerEvents.EfCode
{
    public class SqlEventsDbContext : DbContext
    {
        private readonly IEventsRunner _eventsRunner;

        public SqlEventsDbContext(DbContextOptions<SqlEventsDbContext> options, IEventsRunner eventsRunner)      
            : base(options)
        {
            _eventsRunner = eventsRunner;
        }

        public DbSet<BookWithEvents> Books { get; set; }
        public DbSet<AuthorWithEvents> Authors { get; set; }
        public DbSet<BookAuthorWithEvents> BookAuthors { get; set; }



        //I only have to override these two version of SaveChanges, as the other two versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (_eventsRunner == null)
                return base.SaveChanges(acceptAllChangesOnSuccess);

            var trackedEntities = ChangeTracker.Entries().ToList();

            return _eventsRunner.RunEventsBeforeAfterSaveChanges(() => ChangeTracker.Entries<EventsHolder>(),
                    () => base.SaveChanges(acceptAllChangesOnSuccess));
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsRunner == null)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return await _eventsRunner.RunEventsBeforeAfterSaveChangesAsync(() => ChangeTracker.Entries<EventsHolder>(),
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookWithEventsConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorWithEventsConfig());
            modelBuilder.ApplyConfiguration(new AuthorWithEventsConfig());
        }
    }
}

