// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.EfCode;
using DataLayer.NoSqlCode.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataLayer.NoSqlCode
{
    public class NoSqlBookUpdater : IBookUpdater
    {
        private readonly ApplyChangeToNoSql _applier;

        public NoSqlBookUpdater(SqlDbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _applier = new ApplyChangeToNoSql(sqlContext, noSqlContext);
        }

        public void IfBookListChangesThenUpdateNoSql(IEnumerable<EntityEntry> changes)
        {
            var bookChanges = BookChangeInfo.FindBookChanges(changes);
            _applier.UpdateNoSql(bookChanges);
        }
    }
}