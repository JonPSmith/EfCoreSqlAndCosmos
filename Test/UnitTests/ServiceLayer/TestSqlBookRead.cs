// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using ServiceLayer.BooksSql.QueryObjects;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestSqlBookRead
    {
        private ITestOutputHelper _output;

        public TestSqlBookRead(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestBookListDtoSelect()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var dtos = context.Books.MapBookToDto().OrderBy(x => x.PublishedOn).ToList();

                

                //VERIFY
                dtos.Select(x => x.AuthorsOrdered).ShouldEqual(new [] { "Martin Fowler", "Martin Fowler", "Eric Evans", "Future Person" });
                dtos.Select(x => x.ReviewsAverageVotes).ShouldEqual(new double? [] { null, null, null, 5 });
            }
        }

    }
}