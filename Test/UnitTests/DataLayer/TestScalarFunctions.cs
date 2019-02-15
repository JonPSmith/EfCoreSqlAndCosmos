// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.SqlCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;
using ApplyScriptExtension = TestSupport.EfHelpers.ApplyScriptExtension;

namespace Test.UnitTests.DataLayer
{
    public class TestScalarFunctions
    {
        private ITestOutputHelper _output;

        public TestScalarFunctions(ITestOutputHelper output)
        {
            _output = output;

            var options = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                var filepath = TestData.GetFilePath(@"..\..\EfCoreSqlAndCosmos\wwwroot\AddUserDefinedFunctions.sql");
                ApplyScriptExtension.ExecuteScriptFileInTransaction(context, filepath);
                context.WipeAllDataFromDatabase();

                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
            }
        }



        [Fact]
        public void TestAverageDbFunctionOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptionsWithLogging<SqlDbContext>(
                log => _output.WriteLine(log.Message));
            using (var context = new SqlDbContext(options))
            {

                //ATTEMPT
                var aveReviews = context.Books
                    .Select(p => UdfDefinitions.AverageVotesUdf(p.BookId))
                    .ToList();

                //VERIFY
                aveReviews.First().ShouldEqual(3);
            }
        }

        [Fact]
        public void TestAuthorsStringDbFunctionOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptionsWithLogging<SqlDbContext>(
                log => _output.WriteLine(log.Message));
            using (var context = new SqlDbContext(options))
            {

                //ATTEMPT
                var aveReviews = context.Books
                    .Select(p => UdfDefinitions.AuthorsStringUdf(p.BookId))
                    .ToList();

                //VERIFY
                aveReviews.First().ShouldEqual("Author1, Author2");
            }
        }

    }
}