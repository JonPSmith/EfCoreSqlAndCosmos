// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
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
                ex.Message.ShouldStartWith("The property 'CosmosBook.CosmosBookId' does not have a value set and no value generator is available for properties of type 'int'.");
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

                //ATTEMPT
                var ex = await Assert.ThrowsAsync<DbUpdateException> (async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldContain("Conflicts were detected for item with id 'CosmosBook|1'.");
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
                var books = await context.Books.OrderBy(x => x.NullableInt).ToListAsync();

                //VERIFY
                books[0].NullableInt.ShouldBeNull();
                books[1].NullableInt.ShouldNotBeNull();
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
                var books = await context.Books.OrderBy(x => x.Title).ToListAsync();

                //VERIFY
                books[0].Title.ShouldBeNull();
                books[1].Title.ShouldNotBeNull();
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
                var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldContain("Conflicts were detected for item with id 'CosmosBook|1'.");

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