// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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

        public NoSqlBookUpdater(SqlDbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            _noSqlContext = noSqlContext ?? throw new ArgumentNullException(nameof(noSqlContext));
            _applier = new ApplyChangeToNoSql(sqlContext, noSqlContext);
        }

        /// <summary>
        /// This MUST be called before SavChanges. It finds any Book changes 
        /// </summary>
        /// <returns>true if there are BookChanges that need projecting to NoSQL database</returns>
        public bool FoundBookChangesToProjectToNoSql()
        {
            _bookChanges = BookChangeInfo.FindBookChanges(_sqlContext.ChangeTracker.Entries());
            return _bookChanges.Any();
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
        public int ExecuteTransactionToSaveBookUpdates(bool acceptAllChangesOnSuccess)
        {
            var strategy = _sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                return strategy.Execute(() => RunSqlTransactionWithNoSqlWrite(acceptAllChangesOnSuccess));
            }

            return RunSqlTransactionWithNoSqlWrite(acceptAllChangesOnSuccess);
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
        public async Task<int> ExecuteTransactionToSaveBookUpdatesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var strategy = _sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                return await strategy.ExecuteAsync(async () => 
                    await RunSqlTransactionWithNoSqlWriteAsync(acceptAllChangesOnSuccess, cancellationToken));
            }

            return await RunSqlTransactionWithNoSqlWriteAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        //--------------------------------------------------------------
        //private methods

        private int RunSqlTransactionWithNoSqlWrite(bool acceptAllChangesOnSuccess)
        {
            if (_sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            using (var transaction = _sqlContext.Database.BeginTransaction())
            {
                var result =_sqlContext
                    .SaveChanges(acceptAllChangesOnSuccess); //Save the SQL changes
                _applier.UpdateNoSql(_bookChanges);          //apply the book changes to the NoSql database
                _noSqlContext.SaveChanges();                 //And Save to NoSql database
                transaction.Commit();
                return result;
            }
        }

        private async Task<int> RunSqlTransactionWithNoSqlWriteAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            if (_sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            using (var transaction = _sqlContext.Database.BeginTransaction())
            {
                var result = await _sqlContext
                    .SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);//Save the SQL changes
                await _applier.UpdateNoSqlAsync(_bookChanges);                      //apply the book changes to the NoSql database
                await _noSqlContext.SaveChangesAsync(cancellationToken);            //And Save to NoSql database
                transaction.Commit();
                return result;
            }
        }
    }
}