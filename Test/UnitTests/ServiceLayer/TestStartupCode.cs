// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DatabaseServices.Concrete;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestStartupCode
    {

        [Fact]
        public void TestSeedDatabaseOk()
        {
            //SETUP
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(sqlOptions))
            {
                context.Database.EnsureCreated();

                var rootDir = TestData.GetTestDataDir();

                //ATTEMPT
                var numBooks = context.SeedDatabase(rootDir);

                //VERIFY
                var books= context.Books.Include(x => x.Reviews).ToList();
                books.Count.ShouldEqual(4);
                books.Count(x => x.ReviewsCount > 0).ShouldEqual(3);
                books.ForEach(x => x.AuthorsOrdered.ShouldNotBeNull());
            }
        }



    }
}