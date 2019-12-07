// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using GenericServices;
using GenericServices.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksSql;
using ServiceLayer.BooksSql.Dtos;
using ServiceLayer.BooksSqlWithEvents;
using ServiceLayer.DatabaseServices.Concrete;
using ServiceLayer.Logger;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class SqlEventsController : BaseTraceController
    {
        public IActionResult Index (SqlSortFilterPageOptions options, [FromServices]ISqlEventsListBooksService service)
        {
            var output = service.SortFilterPage(options).ToList();
            SetupTraceInfo();
            return View(new SqlBookListCombinedDto(options, output));              
        }

        /// <summary>
        /// This provides the filter search dropdown content
        /// </summary>
        /// <param name="options"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetFilterSearchContent    
            (SqlSortFilterPageOptions options, [FromServices]IBookEventsFilterDropdownService service)         
        {

            var traceIdent = HttpContext.TraceIdentifier; 
            return Json(                            
                new TraceIndentGeneric<IEnumerable<DropdownTuple>>(
                traceIdent,
                service.GetFilterDropDownValues(  options.FilterBy)));            
        }

        //-------------------------------------------------------

        public IActionResult ChangePubDate(Guid id, [FromServices]ICrudServices<SqlDbContext> service)
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
        public async Task<IActionResult> ChangePubDate(ChangePubDateDto dto, [FromServices]ICrudServicesAsync<SqlDbContext> service)
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

        public IActionResult AddPromotion(Guid id, [FromServices]ICrudServices<SqlDbContext> service)
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
        public IActionResult AddPromotion(AddRemovePromotionDto dto, [FromServices]ICrudServices<SqlDbContext> service)
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

        public IActionResult RemovePromotion(Guid id, [FromServices]ICrudServices<SqlDbContext> service)
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
        public IActionResult RemovePromotion(AddRemovePromotionDto dto, [FromServices]ICrudServices<SqlDbContext> service)
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


        public IActionResult AddBookReview(Guid id, [FromServices]ICrudServices<SqlDbContext> service)
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
        public IActionResult AddBookReview(AddReviewDto dto, [FromServices]ICrudServices<SqlDbContext> service)
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

    }
}
