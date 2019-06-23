using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BudgetTools.Controllers.Angular
{
    public class ImportController : Controller
    {

        public ActionResult Index()
        {
            return View("~/Views/Import/Import.cshtml");
        }

    }
}