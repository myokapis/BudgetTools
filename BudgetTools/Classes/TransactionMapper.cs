using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using BudgetTools.Models.DomainModels;

namespace BudgetTools.Transactions
{
  // maps enumerable transaction data to a Transaction object
  // TODO: make this configurable perhaps even with an interface and separate definitions for each file format
  public class TransactionMapper
  {

    public static StageTransaction Map(string[] data, int BankAccountId)
    {
      string[] row = CleanData(data);

      StageTransaction t = new StageTransaction
      {
        BankAccountId = BankAccountId,
        TransactionNo = GetTransNo(row),
        TransactionDate = GetDate(row),
        TransactionDesc = GetDescription(row),
        CheckNo = GetCheckNo(row),
        Amount = GetAmount(row)
      };

      return t;
    }

    protected static string[] CleanData(string[] data)
    {
      string[] cleanData = new string[data.Length];

      for (int i = 0; i < data.Length; i++)
      {
        cleanData[i] = data[i].Replace("\"", String.Empty);
      }

      return cleanData;
    }

    protected static double GetAmount(string[] row)
    {
      if (row[4].Trim() != String.Empty)
        return Convert.ToDouble(row[4]);
      else if (row[5].Trim() != String.Empty)
        return Convert.ToDouble(row[5]);
      else
        return Convert.ToDouble(row[8]);
    }

    protected static int? GetCheckNo(string[] row)
    {
      if (row[7].Trim() == String.Empty)
        return new int?();
      else
        return new int?(Convert.ToInt32(row[7]));
    }

    protected static DateTime GetDate(string[] row)
    {
      return DateTime.Parse(row[1]);
    }

    protected static string GetDescription(string[] row)
    {
      string string1 = row[2];
      string string2 = row[3];
      StringBuilder sbOut = new StringBuilder();
      string pattern = "^(\\d+\\s*)+";

      Match match = Regex.Match(string1, pattern);

      if (match.Success)
      {
        int matchLen = match.Value.Length;
        sbOut.Append(string1.Substring(matchLen, string1.Length - matchLen));
        sbOut.Append(" ");
        sbOut.Append(string2);
        sbOut.Append(" (");
        sbOut.Append(match.Value.Trim());
        sbOut.Append(")");
        return sbOut.ToString();
      }
      else
      {
        return String.Concat(string1, " ", string2);
      }
    }

    protected static string GetTransNo(string[] row)
    {
      return row[0];
    }
  }
}