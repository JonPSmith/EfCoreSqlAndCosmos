// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.SqlCode
{
    public static class UdfDefinitions
    {
        public const string SqlScriptName = "AddUserDefinedFunctions.sql";

        public static void RegisterUdfDefintions(this ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(
                    () => AuthorsStringUdf(default(Guid)))
                .HasSchema("dbo");
        }

        public static double? AverageVotesUdf(Guid bookId)
        {
            throw new Exception();
        }

        public static string AuthorsStringUdf(Guid bookId)
        {
            throw new NotImplementedException(
                "Called by client vs. server evaluation.");
        }
    }
}