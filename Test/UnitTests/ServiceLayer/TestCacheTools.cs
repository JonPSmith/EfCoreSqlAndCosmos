// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayerEvents.EfCode;
using ServiceLayer.BooksSqlWithEvents.Services;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestCacheTools
    {
        private ITestOutputHelper _output;

        public TestCacheTools(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void TestCheckUpdateBookCachePropertiesOk()
        {
            //SETUP
            var sqlOptions = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            using (var context = new SqlEventsDbContext(sqlOptions, null))
            {
                context.Database.EnsureCreated();

                var books = DddEfTestData.CreateDummyBooksWithEvents(2);
                books[0].AuthorsOrdered = null;
                context.AddRange(books);
                context.SaveChanges();

                var service = new CacheToolsService(context);

                //ATTEMPT
                var status = service.CheckUpdateBookCacheProperties();

                //VERIFY
                status.Message.ShouldEqual("Processed 2 books and 2 errors found. See returned string for details");
                _output.WriteLine(status.Result);
            }
        }



    }
}