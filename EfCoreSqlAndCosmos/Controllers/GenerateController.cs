// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class GenerateController : BaseTraceController
    {
        //This is a hack. Shouldn't use static variables like this! Not multi-user safe!!
        private static double _progress;
        private static bool _cancel;

        // GET
        public IActionResult Index([FromServices]SqlDbContext context)
        {
            _progress = 0;
            _cancel = false;
            return View(context.Books.Count());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Books(int totalBooksNeeded, bool wipeDatabase, 
            [FromServices]SqlDbContext sqlContext,
            [FromServices]NoSqlDbContext noSqlContext,
            [FromServices]BookGenerator generator,
            [FromServices]IWebHostEnvironment env)
        {
            if (totalBooksNeeded == 0)
                return View((object) "Error: should contain the number of books to generate.");

            if (wipeDatabase)
                sqlContext.DevelopmentWipeCreated(noSqlContext);

            var filepath = Path.Combine(env.WebRootPath, SetupHelpers.SeedFileSubDirectory,
                SetupHelpers.TemplateFileName);
            await generator.WriteBooksAsync(filepath,
                totalBooksNeeded, true, numWritten =>
            {
                _progress = numWritten * 100.0 / totalBooksNeeded;
                return _cancel;
            });

            SetupTraceInfo();

            return
                View((object) ((_cancel ? "Cancelled" : "Successful") +
                     $" generate. Num books in database = {sqlContext.Books.Count()}."));
        }

        [HttpPost]
        public ActionResult Progress(bool cancel, [FromServices]SqlDbContext context)
        {
            _cancel = cancel;
            return Content("999");
        }
    }
}