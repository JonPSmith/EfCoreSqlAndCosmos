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

        public static void DevelopmentEnsureCreated(this SqlDbContext db, string wwwrootDirectory)
        {
            db.Database.EnsureCreated();
            //var filepath = Path.Combine(wwwrootDirectory, UdfDefinitions.SqlScriptName);
            //db.ExecuteScriptFileInTransaction(filepath);
        }

        public static void DevelopmentWipeCreated(this SqlDbContext db, string wwwrootDirectory)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.DevelopmentEnsureCreated(wwwrootDirectory);
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
                //We add this separately so that it has the highest Id. That will make it appear at the top of the default list
                context.Books.Add(SpecialBook.CreateSpecialBook());
                context.SaveChanges();
                numBooks = books.Count + 1;
            }

            return numBooks;
        }

        public static void GenerateBooks(this DbContextOptions<SqlDbContext> options,
            int numBooksToAdd, string wwwrootDirectory, Func<int, bool> progessCancel)
        {
            //add generated books
            var gen = new BookGenerator(Path.Combine(wwwrootDirectory, SeedFileSubDirectory, TemplateFileName),true);
            gen.WriteBooks(numBooksToAdd, options, progessCancel);
        }
    }
}