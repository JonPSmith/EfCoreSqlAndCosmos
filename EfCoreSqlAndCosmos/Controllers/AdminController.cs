// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using EfCoreInAction.Controllers;
using GenericServices;
using GenericServices.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Books.Dtos;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class AdminController : BaseTraceController
    {
        public IActionResult ChangePubDate (int id, [FromServices]ICrudServices service) 
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
        public IActionResult ChangePubDate(ChangePubDateDto dto, [FromServices]ICrudServices service)
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

        public IActionResult AddPromotion(int id, [FromServices]ICrudServices service)
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

        public IActionResult RemovePromotion(int id, [FromServices]ICrudServices service)
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


        public IActionResult AddBookReview(int id, [FromServices]ICrudServices service)
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

        public IActionResult ResetDatabase([FromServices]SqlDbContext context, [FromServices]IHostingEnvironment env)
        {
           
            context.DevelopmentWipeCreated(env.WebRootPath);
            var numBooks = context.SeedDatabase(env.WebRootPath);
            SetupTraceInfo();
            return View("BookUpdated", $"Successfully reset the database and added {numBooks} books.");
        }
    }
}
