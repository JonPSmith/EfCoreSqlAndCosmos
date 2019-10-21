// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.NoSqlCode
{
    public interface IBookUpdater
    {
        int FindNumBooksChanged(SqlDbContext sqlContext);
        int CallBaseSaveChangesAndNoSqlWriteInTransaction(DbContext sqlContext, int bookChanges, Func<int> callBaseSaveChanges);
        Task<int> CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(DbContext sqlContext, int bookChanges, Func<Task<int>> callBaseSaveChangesAsync);
    }
}