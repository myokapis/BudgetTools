using System;
using System.Text;
using System.Text.RegularExpressions;

namespace BudgetToolsBLL.Models
{
    // TODO: consider making this another DTO and moving the logic to an appropriate place
    public class StagedTransaction
    {

        public StagedTransaction() { }

        public StagedTransaction(int bankAccountId, string[] data)
        {
            this.Amount = double.TryParse(GetValue(data[4], data[5], data[8]), out var amount) ? amount : 0d;
            this.Balance = double.TryParse(data[6], out var balance) ? balance : 0d;
            this.BankAccountId = bankAccountId;
            this.CheckNo = string.IsNullOrWhiteSpace(data[7]) ? new int?() : int.Parse(data[7]);
            this.TransactionDate = DateTime.Parse(data[1]);
            this.TransactionDesc = GetDescription(data);
            this.TransactionNo = data[0];
        }

        public double Amount { get; set; }
        public double Balance { get; set; }
        public int BankAccountId { get; set; }
        public int? CheckNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionDesc { get; set; }
        public string TransactionNo { get; set; }

        protected string GetDescription(string[] data)
        {
            string string1 = data[2];
            string string2 = data[3];
            string pattern = "^(\\d+\\s*)+";

            Match match = Regex.Match(string1, pattern);

            if (match.Success)
            {
                StringBuilder sbOut = new StringBuilder();
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
                return string.Concat(string1, " ", string2);
            }
        }

        protected string GetValue(params string[] items)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item)) return item;
            }

            return "";
        }

    }

}
