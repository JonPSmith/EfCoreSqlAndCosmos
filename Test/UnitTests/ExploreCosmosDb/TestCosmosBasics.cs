// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Test.CosmosTestDb;
using Test.Helpers;
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
        public async Task TestAddCosmosBookKeyRulesOk()
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
                var cBook = new CosmosBook { CosmosBookId = 1, Title = "Book2" };
                context.Add(cBook);
                await context.SaveChangesAsync();

                //VERIFY
                context.Books.Find(1).Title.ShouldEqual("Book2");
            }
        }

        [Fact]
        public async Task TestAddCosmosBookWithReviewsOk()
        {
            //SETUP
            var options = this.GetCosmosDbToEmulatorOptions<CosmosDbContext>();
            using (var context = new CosmosDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var cBook = new CosmosBook
                {
                    CosmosBookId = 1, Title = "Book",
                    Reviews = new List<CosmosReview>
                    {
                        new CosmosReview{Comment = "Great!", NumStars = 5, VoterName = "Reviewer1"},
                        new CosmosReview{Comment = "Hated it", NumStars = 1, VoterName = "Reviewer2"}
                    }
                };
                context.Add(cBook);
                await context.SaveChangesAsync();

                //VERIFY
                context.Books.Find(1).Reviews.Count.ShouldEqual(2);
            }
        }
    }
}