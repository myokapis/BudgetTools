using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using BudgetTools.Models;
using BudgetTools.Models.DomainModels;
using BudgetTools.Models.ViewModels;
using BudgetTools.Transactions;

namespace BudgetTools.Controllers
{
    public class TransactionController : Controller
    {
      //
      // GET: /Import/
      public ActionResult Index()
      {
        BudgetToolsDBContext db = new BudgetToolsDBContext();

        //// find first open period date
        //var filterDate = db.Periods.Where(p => p.IsOpen == true).Min(p => p.PeriodStartDate);

        // setup the transaction types list
        var transactionTypes = db.TransactionTypes; //.Where(tt => tt.TransactionTypeCode != "I");
        List<SelectListItem> typeList = new List<SelectListItem>();
        foreach (TransactionType transactionType in transactionTypes)
        {
          typeList.Add(new SelectListItem { Text = transactionType.TransactionTypeDesc, Value = transactionType.TransactionTypeCode, Selected = (transactionType.TransactionTypeCode == "I") });
        }
        ViewData["TransactionType"] = typeList;

        // setup the budget lines list
        var budgetLines = db.BudgetLines.OrderBy(bl => bl.DisplayName);
        List<SelectListItem> budgetLineList = new List<SelectListItem>();
        budgetLineList.Add(new SelectListItem { Text = "---", Value = "-1", Selected = true });
        foreach (BudgetLine budgetLine in budgetLines)
        {
          budgetLineList.Add(new SelectListItem { Text = budgetLine.DisplayName, Value = budgetLine.BudgetLineId.ToString() });
        }
        ViewData["BudgetLine"] = budgetLineList;

        var transactions = db.Transactions
          .Where(t => t.TransactionDate >= db.CurrentPeriod.PeriodStartDate) //.Where(t => !t.IsMapped);
          .OrderByDescending(t => t.TransactionDate).OrderByDescending(t => t.TransactionNo);
        //ViewData["FirstTransactionId"] = transactions.OrderBy(t => t.TransactionId).First().TransactionId;
        return View(transactions);
      }
     
      [HttpGet]
      public JsonResult LoadEditor(int TransactionId)
      {
        //BudgetToolsDBContext db = new BudgetToolsDBContext();
        //var transaction = db.Transactions.Include("MappedTransactions")
        //  .FirstOrDefault(t => t.TransactionId == TransactionId);
        //MappedTransaction mappedTransaction = transaction.MappedTransactions.FirstOrDefault();

        //// ensure the transaction to be edited has a mapped transaction
        //if (mappedTransaction == null)
        //{
        //  mappedTransaction = new MappedTransaction();
        //  //mappedTransaction.TransactionId = transaction.TransactionId;
        //  //mappedTransaction.Amount = transaction.Amount;
        //  //transaction.MappedTransactions.Add(mappedTransaction);
        //}

        //TransactionViewModel viewModel = new TransactionViewModel();
        //viewModel.BudgetLine1Id = mappedTransaction.BudgetLineId;
        //viewModel.Notes = transaction.Notes;
        //viewModel.Recipient = transaction.Recipient;
        //viewModel.TransactionId = transaction.TransactionId;
        //viewModel.TransactionTypeCode = transaction.TransactionTypeCode;

        return this.Json(TransactionXRef.Get(TransactionId), JsonRequestBehavior.AllowGet);
      }

      public JsonResult SaveTransaction(TransactionViewModel viewModel)
      {
        TransactionXRef.Map(viewModel);
        return this.Json("");
      }

      public ActionResult SelectFile()
      {
        BudgetToolsDBContext db = new BudgetToolsDBContext();

        // setup the bank account list
        var bankAccounts = db.BankAccounts;
        List<SelectListItem> bankAccountList = new List<SelectListItem>();
        bankAccountList.Add(new SelectListItem { Text = "---", Value = "", Selected = true });
        foreach (BankAccount bankAccount in bankAccounts)
        {
          bankAccountList.Add(new SelectListItem { Text = bankAccount.BankAccountName, Value = bankAccount.BankAccountId.ToString() });
        }
        ViewData["BankAccount"] = bankAccountList;
        return View();
      }

      public ActionResult Upload(HttpPostedFileBase file)
      {
        try
        {
          if (file.ContentLength > 0)
          {
            string path = String.Concat(HttpContext.Request.MapPath("~/temp/"), file.FileName);
            file.SaveAs(path);
            int intBankAccountId = Convert.ToInt32(Request["BankAccount"]);
            TransactionParser parser = new TransactionParser(path, intBankAccountId);
            parser.Parse();
            ViewBag.Message = "Upload succeeded";
            return RedirectToAction("Index");
          }
          else
          {
            ViewBag.Message = "Upload failed.";
            return RedirectToAction("SelectFile");
          }
        }
        catch(Exception e)
          {
            ViewBag.Message = e.Message;
            return RedirectToAction("SelectFile");
          }
      }
	}
}