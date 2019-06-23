//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using BudgetTools.Models.DomainModels;
//using BudgetTools.Models.ViewModels;

//namespace BudgetTools.Models
//{
//  public class TransactionXRef
//  {

//    public static void Map(TransactionViewModel viewModel)
//    {
//      List<MappedTransaction> mappedTransactions;
//      MappedTransaction mappedTransaction;
//      BudgetToolsDBContext db = new BudgetToolsDBContext();
//      bool blnAdd = false;

//      // transform the view model data to arrays of data
//      int[] budgetLines = new int[] {viewModel.BudgetLine1Id, viewModel.BudgetLine2Id, viewModel.BudgetLine3Id};
//      decimal[] amounts = new decimal[] {viewModel.Amount1, viewModel.Amount2, viewModel.Amount3};

//      // get the transaction
//      var transaction = db.Transactions.Include("MappedTransactions")
//        .First(t => t.TransactionId == viewModel.TransactionId);

//      // update the transaction
//      transaction.TransactionTypeCode = viewModel.TransactionTypeCode;
//      transaction.Recipient = (viewModel.Recipient == "") ? null : viewModel.Recipient;
//      transaction.Notes = (viewModel.Notes == "") ? null : viewModel.Notes;
//      transaction.IsMapped = true;

//      mappedTransactions = transaction.MappedTransactions.ToList<MappedTransaction>();

//      for(int i = 0; i< 3; i++)
//      {
//        mappedTransaction = (mappedTransactions.Count > i) ? mappedTransactions[i] : null;
        
//        if (budgetLines[i] <= 0)
//        {
//          // remove the mapped transaction if it exists in the db but no longer exists on the web
//          if (mappedTransaction != null) mappedTransactions.Remove(mappedTransaction);
//        }
//        else
//        {
//          // create a new transaction if one was not found
//          if (mappedTransaction == null)
//          {
//            mappedTransaction = new MappedTransaction();
//            blnAdd = true;
//          }

//          // set the mapped transaction values
//          mappedTransaction.Amount = amounts[i];
//          mappedTransaction.BudgetLineId = budgetLines[i];
//          mappedTransaction.TransactionId = viewModel.TransactionId;

//          // add the mapped transaction if it is new
//          if (blnAdd) transaction.MappedTransactions.Add(mappedTransaction);
//        }
//      }

//      db.SaveChanges();
//    }

//    public static TransactionViewModel Get(int TransactionId)
//    {
//      BudgetToolsDBContext db = new BudgetToolsDBContext();
//      TransactionViewModel viewModel = new TransactionViewModel();
//      MappedTransaction mappedTransaction;

//      var transaction = db.Transactions.Include("MappedTransactions")
//        .First(t => t.TransactionId == TransactionId);

//      int[] budgetLines = new int[] { -1, -1, -1 };
//      decimal[] amounts = new decimal[] { transaction.Amount, 0, 0 };

//      List<MappedTransaction> mappedTransactions = transaction.MappedTransactions.ToList();

//      viewModel.TransactionId = transaction.TransactionId;
//      viewModel.TransactionTypeCode = transaction.TransactionTypeCode;
//      viewModel.Notes = transaction.Notes;
//      viewModel.Recipient = transaction.Recipient;
//      viewModel.TotalAmount = transaction.Amount;

//      for (int i = 0; i < 3; i++)
//      {
//        if (mappedTransactions.Count > i)
//        {
//          mappedTransaction= mappedTransactions[i];
//          budgetLines[i] = mappedTransaction.BudgetLineId;
//          amounts[i] = mappedTransaction.Amount;
//        }
//      }

//      viewModel.BudgetLine1Id = budgetLines[0];
//      viewModel.Amount1 = amounts[0];
//      viewModel.BudgetLine2Id = budgetLines[1];
//      viewModel.Amount2 = amounts[1];
//      viewModel.BudgetLine3Id = budgetLines[2];
//      viewModel.Amount3 = amounts[2];

//      return viewModel;
//    }

//  }
//}