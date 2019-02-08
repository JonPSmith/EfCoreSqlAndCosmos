// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.NoSqlCode
{
    public interface IBookUpdater
    {
        bool FoundBookChangesToProjectToNoSql(DbContext sqlContext);
        int ExecuteTransactionToSaveBookUpdates(DbContext sqlContext, bool acceptAllChangesOnSuccess);
        Task<int> ExecuteTransactionToSaveBookUpdatesAsync(DbContext sqlContext, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);
    }
}