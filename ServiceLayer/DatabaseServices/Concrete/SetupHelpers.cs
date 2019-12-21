// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.SqlCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DatabaseCode.Services;

namespace ServiceLayer.DatabaseServices.Concrete
{
    public enum DbStartupModes { UseExisting, EnsureCreated, EnsureDeletedCreated, UseMigrations}

    public static class SetupHelpers
    {
        private const string SeedDataSearchName = "Apress books*.json";
        public const string TemplateFileName = "Manning books.json";
        public const string SeedFileSubDirectory = "seedData";

        public static void DevelopmentEnsureCreated(this SqlDbContext sqlDbContext, NoSqlDbContext noSqlDbContext)
        {
            sqlDbContext.Database.EnsureCreated();
            noSqlDbContext?.Database.EnsureCreated();
        }

        public static void DevelopmentWipeCreated(this SqlDbContext sqlDbContext, NoSqlDbContext noSqlDbContext)
        {
            sqlDbContext.Database.EnsureDeleted();
            noSqlDbContext?.Database.EnsureDeleted();
            sqlDbContext.DevelopmentEnsureCreated(noSqlDbContext);
        }

        public static int SeedDatabase(this SqlDbContext context, string wwwrootDirectory)
        {
            var numBooks = context.Books.Count();
            if (numBooks == 0)
            {
                //the database is empty so we fill it from a json file
                //This also sets up the NoSql database IF the IBookUpdater member is registered.
                var books = BookJsonLoader.LoadBooks(Path.Combine(wwwrootDirectory, SeedFileSubDirectory),
                    SeedDataSearchName).ToList();
                context.Books.AddRange(books);
                context.SaveChanges();

                //we add the special book 
                context.Books.Add(SpecialBook.CreateSpecialBook());
                context.SaveChanges();
                numBooks = books.Count + 1;
            }

            return numBooks;
        }

    }
}