using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{
    public class BalancesController : Controller
    {
        protected IBalancesPresenter balancesPresenter;
        protected IBudgetService budgetService;

        public BalancesController(IBalancesPresenter balancesPresenter, IBudgetService budgetService)
        {
            this.balancesPresenter = balancesPresenter;
            this.budgetService = budgetService;
        }

        public ActionResult ChangePeriod(int periodId)
        {
            (Session["pageScope"] as IPageScope).PeriodId = periodId;
            return Content(balancesPresenter.GetTransactionRows());
        }

        public ActionResult Index()
        {
            return Content(balancesPresenter.GetPage());
        }

    }
}