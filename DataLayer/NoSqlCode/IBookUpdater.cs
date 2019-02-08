// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataLayer.NoSqlCode
{
    public interface IBookUpdater
    {
        bool FoundBookChangesToProjectToNoSql();
        int ExecuteTransactionToSaveBookUpdates(bool acceptAllChangesOnSuccess);
        Task<int> ExecuteTransactionToSaveBookUpdatesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);
    }
}