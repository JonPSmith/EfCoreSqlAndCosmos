// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Test.CosmosTestDb;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ExploreCosmosDb
{
    public class TestCosmosBasics
    {
        private ITestOutputHelper _output;

        public TestCosmosBasics(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestAddCosmosBookHaveToSetKeyOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var cBook = new CosmosBook {CosmosBookId = 0, Title = "Book"};
                var ex = Assert.Throws<NotSupportedException>(() => context.Add(cBook));

                //VERIFY
                ex.Message.ShouldStartWith("The 'CosmosBookId' on entity type 'CosmosBook' does not have a value set and no value generator is available for properties of type 'int'.");
            }
        }

        [Fact]
        public async Task TestAnyOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                var cBook1 = new CosmosBook { CosmosBookId = 1, Title = "Book1" };
                var cBook2 = new CosmosBook { CosmosBookId = 2, Title = "Book2" };
                context.AddRange(cBook1, cBook2);
                await context.SaveChangesAsync();

                //ATTEMPT
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Books.AnyAsync(x => x.Title == "Book2"));

                //VERIFY
                ex.Message.ShouldStartWith("The LINQ expression 'Any<CosmosBook>(\r\n    source: DbSet<CosmosBook>, \r\n    predicate: (c) => c.Title == \"Book2\")' could not be translated.");
            }
        }

        [Fact]
        public async Task TestAddCosmosBookHowToUpdateOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook = new CosmosBook {CosmosBookId = 1, Title = "Book1"};
                context.Add(cBook);
                await context.SaveChangesAsync();

            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var cBook = await context.Books.FindAsync(1); //You must read to get the "id"
                cBook.Title = "Book2";
                await context.SaveChangesAsync();
            }
            using (var context = new CosmosDbContext(options))
            {
                //VERIFY
                context.Books.Find(1).Title.ShouldEqual("Book2");
            }
        }

        [Fact]
        public async Task TestNullableIntOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook1 = new CosmosBook { CosmosBookId = 1, NullableInt = null};
                var cBook2 = new CosmosBook { CosmosBookId = 2, NullableInt = 1 };
                context.AddRange(cBook1, cBook2);
                await context.SaveChangesAsync();
            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var cBook1 = await context.Books.FindAsync(1);
                var cBook2 = await context.Books.FindAsync(2);

                //VERIFY
                cBook1.NullableInt.ShouldBeNull();
                cBook2.NullableInt.ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestNullableIntOrderByOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook1 = new CosmosBook { CosmosBookId = 1, NullableInt = null };
                var cBook2 = new CosmosBook { CosmosBookId = 2, NullableInt = 1 };
                context.AddRange(cBook1, cBook2);
                await context.SaveChangesAsync();
            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var ex = await Assert.ThrowsAsync<NotSupportedException>(async ()
                    => await context.Books.OrderBy(x => x.NullableInt).ToListAsync());

                //VERIFY
                ex.Message.ShouldStartWith("Cannot execute cross partition order-by queries on mix types. " +
                                           "Consider using IS_STRING/IS_NUMBER to get around this exception.");
            }
        }

        [Fact]
        public async Task TestStringWithNullOrderByOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook1 = new CosmosBook { CosmosBookId = 1,  };
                var cBook2 = new CosmosBook { CosmosBookId = 2, Title = "Book2"};
                context.AddRange(cBook1, cBook2);
                await context.SaveChangesAsync();
            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var ex = await Assert.ThrowsAsync<NotSupportedException>(async () =>
                    await context.Books.OrderBy(x => x.Title).ToListAsync());

                //VERIFY
                ex.Message.ShouldStartWith("Cannot execute cross partition order-by queries on mix types. " +
                                           "Consider using IS_STRING/IS_NUMBER to get around this exception.");
            }
        }

        [Fact]
        public async Task TestNullableIntWhereOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook1 = new CosmosBook { CosmosBookId = 1, NullableInt = null };
                var cBook2 = new CosmosBook { CosmosBookId = 2, NullableInt = 1 };
                context.AddRange(cBook1, cBook2);
                await context.SaveChangesAsync();
            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var book = await context.Books.SingleOrDefaultAsync(x => x.NullableInt > 0);

                //VERIFY
                book.CosmosBookId.ShouldEqual(2);
            }
        }

        [Fact]
        public async Task TestAddCosmosBookAddSameKeyOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                var cBook = new CosmosBook { CosmosBookId = 1, Title = "Book1" };
                context.Add(cBook);
                await context.SaveChangesAsync();

            }
            using (var context = new CosmosDbContext(options))
            {
                //ATTEMPT
                var cBook = new CosmosBook { CosmosBookId = 1, Title = "Book2" };
                context.Add(cBook);
                context.Entry(cBook).State.ShouldEqual(EntityState.Added);
                var changes = await context.SaveChangesAsync();

                //VERIFY
                context.Entry(cBook).State.ShouldEqual(EntityState.Unchanged);
                changes.ShouldEqual(0);
            }
            using (var context = new CosmosDbContext(options))
            {
                context.Books.Find(1).Title.ShouldEqual("Book1");
            }
        }

        [Fact]
        public async Task TestAddCosmosBookWithReviewsOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using var context = new CosmosDbContext(options);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            //ATTEMPT
            var cBook = new CosmosBook
            {
                CosmosBookId = 1,      //NOTE: You have to provide a key value!
                Title = "Book Title",
                PublishedDate = new DateTime(2019, 1,1),
                Reviews = new List<CosmosReview>
                {
                    new CosmosReview{Comment = "Great!", NumStars = 5, VoterName = "Reviewer1"},
                    new CosmosReview{Comment = "Hated it", NumStars = 1, VoterName = "Reviewer2"}
                }
            };
            context.Add(cBook);
            await context.SaveChangesAsync();

            //VERIFY
            (await context.Books.FindAsync(1)).Reviews.Count.ShouldEqual(2);
        }

        [RunnableInDebugOnly]
        public async Task TestReadCurrentDbOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using var context = new CosmosDbContext(options);

            //ATTEMPT
            var book = await context.Books.FindAsync(2);

            //VERIFY

        }
    }
}