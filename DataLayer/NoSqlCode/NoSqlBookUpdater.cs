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
        private readonly NoSqlDbContext _noSqlContext;

        private IImmutableList<BookChangeInfo> _bookChanges;

        public NoSqlBookUpdater(NoSqlDbContext noSqlContext)
        {
            _noSqlContext = noSqlContext ?? throw new ArgumentNullException(nameof(noSqlContext));
        }

        /// <summary>
        /// This MUST be called before SavChanges. It finds any Book changes 
        /// </summary>
        /// <returns>true if there are BookChanges that need projecting to NoSQL database</returns>
        public bool FoundBookChangesToProjectToNoSql(DbContext sqlContext)
        {
            _bookChanges = BookChangeInfo.FindBookChanges(sqlContext.ChangeTracker.Entries());
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
        public int ExecuteTransactionToSaveBookUpdates(DbContext sqlContext, bool acceptAllChangesOnSuccess)
        {
            var strategy = sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                return strategy.Execute(() => RunSqlTransactionWithNoSqlWrite(sqlContext, acceptAllChangesOnSuccess));
            }

            return RunSqlTransactionWithNoSqlWrite(sqlContext, acceptAllChangesOnSuccess);
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
        public async Task<int> ExecuteTransactionToSaveBookUpdatesAsync(DbContext sqlContext, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var strategy = sqlContext.Database.CreateExecutionStrategy();
            if (strategy.RetriesOnFailure)
            {
                return await strategy.ExecuteAsync(async () => 
                    await RunSqlTransactionWithNoSqlWriteAsync(sqlContext, acceptAllChangesOnSuccess, cancellationToken));
            }

            return await RunSqlTransactionWithNoSqlWriteAsync(sqlContext, acceptAllChangesOnSuccess, cancellationToken);
        }

        //--------------------------------------------------------------
        //private methods

        private int RunSqlTransactionWithNoSqlWrite(DbContext sqlContext, bool acceptAllChangesOnSuccess)
        {
            if (sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            var applier = new ApplyChangeToNoSql(sqlContext, _noSqlContext);
            using (var transaction = sqlContext.Database.BeginTransaction())
            {
                var result =sqlContext
                    .SaveChanges(acceptAllChangesOnSuccess); //Save the SQL changes
                applier.UpdateNoSql(_bookChanges);          //apply the book changes to the NoSql database
                _noSqlContext.SaveChanges();                 //And Save to NoSql database
                transaction.Commit();
                return result;
            }
        }

        private async Task<int> RunSqlTransactionWithNoSqlWriteAsync(DbContext sqlContext, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            if (sqlContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException("You can't use the NoSqlBookUpdater if you are using transactions.");

            var applier = new ApplyChangeToNoSql(sqlContext, _noSqlContext);
            using (var transaction = sqlContext.Database.BeginTransaction())
            {
                var result = await sqlContext
                    .SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);//Save the SQL changes
                await applier.UpdateNoSqlAsync(_bookChanges);                      //apply the book changes to the NoSql database
                await _noSqlContext.SaveChangesAsync(cancellationToken);            //And Save to NoSql database
                transaction.Commit();
                return result;
            }
        }
    }
}