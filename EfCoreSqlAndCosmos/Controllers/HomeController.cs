using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfCoreInAction.Controllers;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Books;
using ServiceLayer.Books.Dtos;
using ServiceLayer.Logger;

namespace EfCoreSqlAndCosmos.Controllers
{
    public class HomeController : BaseTraceController
    {


        public IActionResult Index
        (SortFilterPageOptions options,
            [FromServices] IListBooksService service)
        {
            var output = service.SortFilterPage(options).ToList();
            return View(new BookListCombinedDto(options, output));              
        }


        /// <summary>
        /// This provides the filter search dropdown content
        /// </summary>
        /// <param name="options"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetFilterSearchContent    
            (SortFilterPageOptions options, [FromServices]IBookFilterDropdownService service)         
        {

            var traceIdent = HttpContext.TraceIdentifier; 
            return Json(                            
                new TraceIndentGeneric<IEnumerable<DropdownTuple>>(
                traceIdent,
                service.GetFilterDropDownValues(  options.FilterBy)));            
        }


        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
