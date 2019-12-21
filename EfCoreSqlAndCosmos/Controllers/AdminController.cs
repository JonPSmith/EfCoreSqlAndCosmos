// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using GenericServices;
using GenericServices.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.BooksSql.Dtos;
using ServiceLayer.BooksSqlWithEvents;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class AdminController : BaseTraceController
    {
        //------------------------------------------------
        //Admin commands that are called from the top menu

        public IActionResult ResetCacheValues([FromServices]IHardResetCacheService service)
        {
            var status = service.CheckUpdateBookCacheProperties();
            return View("Message", status.Message);
        }

        public IActionResult ResetDatabase(
            [FromServices]SqlDbContext context,
            [FromServices]NoSqlDbContext noSqlDbContext,
            [FromServices]IWebHostEnvironment env)
        {
            context.DevelopmentWipeCreated(noSqlDbContext);
            var numBooks = context.SeedDatabase(env.WebRootPath);
            SetupTraceInfo();
            return View("Message", $"Successfully reset the database and added {numBooks} books.");
        }
    }
}
