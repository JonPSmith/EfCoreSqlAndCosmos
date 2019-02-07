// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataLayer.NoSqlCode
{
    public class NoSqlBookUpdater : IBookUpdater
    {
        private readonly SqlDbContext _sqlContext;
        private readonly NoSqlDbContext _noSqlContext;
        private readonly ApplyChangeToNoSql _applier;

        private IImmutableList<BookChangeInfo> _bookChanges;

        public NoSqlBookUpdater(SqlDbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            _noSqlContext = noSqlContext;
            _applier = new ApplyChangeToNoSql(sqlContext, noSqlContext);
        }

        public void FindTheChangesBeforeSaveChangesIsCalled()
        {
            if (_noSqlContext == null) return; //if noSQlContext is null then we don't want to check/update

            _bookChanges = BookChangeInfo.FindBookChanges(_sqlContext.ChangeTracker.Entries());
        }

        public async Task UpdateNoSqlIfBooksHaveChangedAsync()
        {
            if (_noSqlContext == null) return; //if noSQlContext is null then we don't want to check/update

            if(await _applier.UpdateNoSqlAsync(_bookChanges))
                //Items were added to the noSql database, so call SaveChanges
                await _noSqlContext.SaveChangesAsync();
        }
    }
}