//using System.Linq;
//using BudgetToolsBLL.Services;
//using BudgetTools.Classes;
//using TemplateEngine;

//namespace BudgetTools.Presenters
//{

//    public interface IPresenterBase
//    {
//        string GetBody();
//        string GetHead();
//        string GetPage();
//        string GetTail();
//    }

//    public abstract class PresenterBase : IPresenterBase
//    {

//        protected IBudgetService budgetService;
//        protected IPageScope pageScope;

//        public abstract string GetBody();
//        public abstract string GetHead();
//        public abstract string GetPage();
//        public abstract string GetTail();

//        public string GetSelector()
//        {
//            // get drop down data
//            var periods = this.budgetService.GetPeriods<Option>().OrderByDescending(p => p.Value).ToList();
//            var bankAccounts = this.budgetService.GetBankAccounts<Option>(true);

//            var definitions = new FieldDefinitions();
//            definitions.SetDropdowns(
//                new DropdownDefinition() { Data = periods, FieldName = "PeriodId", SectionName = "BUDGET_PERIODS" },
//                new DropdownDefinition() { Data = bankAccounts, FieldName = "BankAccountId", SectionName = "BANK_ACCOUNTS" }
//            );

//            var tpl = new Template("Common.tpl");
//            tpl.selectSection("SELECTOR");
//            tpl.setSectionFields<IPageScope>(pageScope, SectionOptions.AppendDeselect, definitions);
//            return tpl.getContent();
//        }

//    }

//}