// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.SqlCode;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
            var filepath = Path.Combine(wwwrootDirectory, UdfDefinitions.SqlScriptName);
            db.ExecuteScriptFileInTransaction(filepath);
        }

        public static void DevelopmentWipeCreated(this SqlDbContext db, string wwwrootDirectory)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.DevelopmentEnsureCreated(wwwrootDirectory);
        }

        public static int SeedDatabase(this SqlDbContext context, string wwwrootDirectory)
        {
            if (!(context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                throw new InvalidOperationException("The database does not exist. If you are using Migrations then run PMC command update-database to create it");

            var numBooks = context.Books.Count();
            if (numBooks == 0)
            {
                //the database is emply so we fill it from a json file
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
    }
}