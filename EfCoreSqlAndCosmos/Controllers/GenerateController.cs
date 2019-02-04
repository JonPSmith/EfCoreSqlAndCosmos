// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreInAction.Controllers
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
        public IActionResult Books(int numBooks, bool wipeDatabase, 
            [FromServices]SqlDbContext context,
            [FromServices]DbContextOptions<SqlDbContext> options,
            [FromServices]IHostingEnvironment env)
        {
            if (numBooks == 0)
                return View((object) "Error: should contain the number of books to generate.");


            if (wipeDatabase)
                context.DevelopmentWipeCreated(env.WebRootPath);
            options.GenerateBooks(numBooks, env.WebRootPath, numWritten =>
            {
                _progress = numWritten * 100.0 / numBooks;
                return _cancel;
            });

            SetupTraceInfo();

            return
                View((object) ((_cancel ? "Cancelled" : "Successful") +
                     $" generate. Num books in database = {context.Books.Count()}."));
        }

        [HttpPost]
        public ActionResult Progress(bool cancel)
        {
            _cancel = cancel;
            return Content(_progress.ToString());
        }
    }
}