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
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class AdminController : BaseTraceController
    {
        public IActionResult ChangePubDate (Guid id, [FromServices]ICrudServices service) 
        {        
            var dto = service.ReadSingle<ChangePubDateDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePubDate(ChangePubDateDto dto, [FromServices]ICrudServicesAsync service)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await service.UpdateAndSaveAsync(dto);
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public IActionResult AddPromotion(Guid id, [FromServices]ICrudServices service)
        {
            var dto = service.ReadSingle<AddRemovePromotionDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPromotion(AddRemovePromotionDto dto, [FromServices]ICrudServices service)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            service.UpdateAndSave(dto, nameof(Book.AddPromotion));
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public IActionResult RemovePromotion(Guid id, [FromServices]ICrudServices service)
        {
            var dto = service.ReadSingle<AddRemovePromotionDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePromotion(AddRemovePromotionDto dto, [FromServices]ICrudServices service)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            service.UpdateAndSave(dto, nameof(Book.RemovePromotion));
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }


        public IActionResult AddBookReview(Guid id, [FromServices]ICrudServices service)
        {
            var dto = service.ReadSingle<AddReviewDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBookReview(AddReviewDto dto, [FromServices]ICrudServices service)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            service.UpdateAndSave(dto);
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        //------------------------------------------------
        //Admin commands that are called from the top menu

        public IActionResult ResetDatabase(
            [FromServices]SqlDbContext context, 
            [FromServices]NoSqlDbContext noSqlDbContext, 
            [FromServices]IWebHostEnvironment env)
        {
           
            context.DevelopmentWipeCreated(noSqlDbContext);
            var numBooks = context.SeedDatabase(env.WebRootPath);
            SetupTraceInfo();
            return View("BookUpdated", $"Successfully reset the database and added {numBooks} books.");
        }
    }
}
