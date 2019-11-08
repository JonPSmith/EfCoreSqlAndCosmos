// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlDbContextTests
{
    public class TestSqlRetryStrategy
    {
        [Fact]
        public void TestHasRetryEnabledOk()
        {
            //SETUP
            var connection = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
            optionsBuilder.UseSqlServer(connection,
                option => option.EnableRetryOnFailure());
            var options = optionsBuilder.Options;
            using (var context = new SqlDbContext(options))
            {

                //ATTEMPT
                var strategy = context.Database.CreateExecutionStrategy();

                //VERIFY
                strategy.RetriesOnFailure.ShouldBeTrue();
            }
        }

        [Fact]
        public void TestNoRetryEnabledOk()
        {
            //SETUP
            var connection = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
            optionsBuilder.UseSqlServer(connection);
            var options = optionsBuilder.Options;
            using (var context = new SqlDbContext(options))
            {

                //ATTEMPT
                var strategy = context.Database.CreateExecutionStrategy();

                //VERIFY
                strategy.RetriesOnFailure.ShouldBeFalse();
            }
        }
    }
}