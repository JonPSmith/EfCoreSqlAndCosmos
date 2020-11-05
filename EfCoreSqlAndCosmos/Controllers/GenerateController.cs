// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class GenerateController : BaseTraceController
    {

        // GET
        public IActionResult Index([FromServices]SqlDbContext context)
        {
            return View(context.Books.Count());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Books(int totalBooksNeeded, bool wipeDatabase, CancellationToken cancellationToken,
            [FromServices]SqlDbContext sqlContext,
            [FromServices]BookGenerator generator,
            [FromServices]IWebHostEnvironment env)
        {
            if (totalBooksNeeded == 0)
                return View((object) "Error: should contain the number of books to generate.");

            NoSqlDbContext noSqlContext = (NoSqlDbContext) HttpContext.RequestServices.GetService(typeof(NoSqlDbContext));

            if (wipeDatabase)
                sqlContext.DevelopmentWipeCreated(noSqlContext);

            var filepath = Path.Combine(env.WebRootPath, SetupHelpers.SeedFileSubDirectory,
                SetupHelpers.TemplateFileName);
            await generator.WriteBooksAsync(filepath, totalBooksNeeded, true, cancellationToken);

            SetupTraceInfo();

            return
                View((object) ((cancellationToken.IsCancellationRequested ? "Cancelled" : "Successful") +
                     $" generate. Num books in database = {sqlContext.Books.Count()}."));
        }

        [HttpPost]
        public ActionResult NumBooks([FromServices]SqlDbContext context)
        {
            var dbExists = (context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
            var message = dbExists ? $"Num books = {context.Books.Count()}" : "database being wiped.";
            return Content(message);
        }
    }
}