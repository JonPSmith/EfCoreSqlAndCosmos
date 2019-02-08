// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode.Internal;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.NoSqlCode
{
    //thanks to jimmy bogard's series https://jimmybogard.com/life-beyond-distributed-transactions-an-apostates-implementation-relational-resources/

    public class NoSqlBookUpdater : IBookUpdater
    {
        private readonly SqlDbContext _sqlContext;
        private readonly NoSqlDbContext _noSqlContext;
        private readonly ApplyChangeToNoSql _applier;

        private IImmutableList<BookChangeInfo> _bookChanges;

        public bool HasUpdatesToApply => _bookChanges?.Any() ?? false;

        public NoSqlBookUpdater(SqlDbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            _noSqlContext = noSqlContext ?? throw new ArgumentNullException(nameof(noSqlContext));
            _applier = new ApplyChangeToNoSql(sqlContext, noSqlContext);
        }

        /// <summary>
        /// This needs to be called before SavChanges is called. It finds any Book changes.
        /// </summary>
        public void FindTheChangesBeforeSaveChangesIsCalled()
        {
            _bookChanges = BookChangeInfo.FindBookChanges(_sqlContext.ChangeTracker.Entries());
        }

        /// <summary>
        /// This method will:
        /// 1) start a transaction on the SQL context
        /// 2) Do a SaveChangesAsync on the SQL context
        /// 3) Then project the Book changes to NoSQL database
        /// 4) ... and call SaveChangesAsync on the NoSQL context
        /// 5) finally commit the transaction
        /// </summary>
        /// <returns></returns>
        public void ExecuteTransactionToSaveBookUpdates()
        {
            var strategy = _sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                strategy.Execute(RunSqlTransactionWithNoSqlWrite);
            }
            else
            {
                RunSqlTransactionWithNoSqlWrite();
            }
        }

        /// <summary>
        /// This method will:
        /// 1) start a transaction on the SQL context
        /// 2) Do a SaveChangesAsync on the SQL context
        /// 3) Then project the Book changes to NoSQL database
        /// 4) ... and call SaveChangesAsync on the NoSQL context
        /// 5) finally commit the transaction
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteTransactionToSaveBookUpdatesAsync()
        {
            var strategy = _sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                await strategy.ExecuteAsync(async () => await RunSqlTransactionWithNoSqlWriteAsync());
            }
            else
            {
                await RunSqlTransactionWithNoSqlWriteAsync();
            }
        }

        //--------------------------------------------------------------
        //private methods

        private void RunSqlTransactionWithNoSqlWrite()
        {
            if (_sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            using (var transaction = _sqlContext.Database.BeginTransaction())
            {
                _sqlContext.SaveChanges();             //Save the SQL changes
                _applier.UpdateNoSql(_bookChanges);    //apply the book changes to the NoSql database
                _noSqlContext.SaveChanges();           //And Save to NoSql database
                transaction.Commit();
            }
        }

        private async Task RunSqlTransactionWithNoSqlWriteAsync()
        {
            if (_sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            using (var transaction = _sqlContext.Database.BeginTransaction())
            {
                await _sqlContext.SaveChangesAsync();             //Save the SQL changes
                await _applier.UpdateNoSqlAsync(_bookChanges);    //apply the book changes to the NoSql database
                await _noSqlContext.SaveChangesAsync();           //And Save to NoSql database
                transaction.Commit();
            }
        }
    }
}