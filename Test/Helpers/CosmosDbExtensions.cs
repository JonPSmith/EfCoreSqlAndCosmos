// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Test.Helpers
{
    public static class CosmosDbExtensions
    {
        public static DbContextOptions<T> GetCosmosDbToEmulatorOptions<T>(this object callingClass,
            bool makeMethodUnique = false, [CallerMemberName] string callingMember = "") where T : DbContext
        {
            var databaseName = callingClass.GetType().Name;
            if (makeMethodUnique)
                databaseName += callingMember;
            var builder = new DbContextOptionsBuilder<T>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    databaseName);
            return builder.Options;
        }
    }
}