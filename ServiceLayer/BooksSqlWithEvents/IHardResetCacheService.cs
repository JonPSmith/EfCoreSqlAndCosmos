// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using StatusGeneric;

namespace ServiceLayer.BooksSqlWithEvents
{
    public interface IHardResetCacheService
    {
        IStatusGeneric<string> CheckUpdateBookCacheProperties();
    }
}