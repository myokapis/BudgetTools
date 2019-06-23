//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.IO;
//using BudgetTools.Models.DomainModels;

//namespace BudgetTools.Transactions
//{
//  // parses a csv file
//  public class TransactionParser
//  {
//    protected string strFilePath;
//    protected int intBankAccountId;
//    protected string strDelimiter;
//    protected int intRowsToSkip;

//    private TransactionParser() { }

//    public TransactionParser(string filePath, int BankAccountId)
//    {
//      strFilePath = filePath;
//      intBankAccountId = BankAccountId;

//      // TODO: move this to the database and look it up
//      strDelimiter = ",";
//      intRowsToSkip = 4;
//    }

//    public void Parse()
//    {
//      var data = from f in File.ReadLines(strFilePath)
//                 .Skip(intRowsToSkip)
//                 select f;

//      BudgetToolsDBContext db = new BudgetToolsDBContext();
//      string[] row;
//      StageTransaction transaction;
//      db.TruncateStagedTransactions();

//      foreach(string s in data)
//      {
//        row = s.Split(new[] { ',' }, StringSplitOptions.None);
//        transaction = TransactionMapper.Map(row, intBankAccountId);
//        db.StageTransactions.Add(transaction);
//      }

//      db.SaveChanges();
//      db.ImportTransactions();

//      return;
//    }
//  }
//}
