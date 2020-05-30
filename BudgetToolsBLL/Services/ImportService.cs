using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;

namespace BudgetToolsBLL.Services
{

    public interface IImportService
    {
        IEnumerable<T> ParseStream<T>(Stream stream, ParserConfig parserConfig);
    }

    public class ImportService : IImportService
    {

        public IEnumerable<T> ParseStream<T>(Stream stream, ParserConfig parserConfig)
        {
            int lineNo = 0;
            int bankAccountId = parserConfig.BankAccountId;
            var delimiter = new char[] { parserConfig.Delimiter };
            var reader = new StreamReader(stream); // assumes simple ansi file for now
            var parser = ParserFactory.GetParser(parserConfig.ParserName);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lineNo++;

                // parse the validator (usually account number) from the file
                if (lineNo == parserConfig.ValidationRowNo)
                {
                    if (parserConfig.ValidationValue != ParseValidator(line, parserConfig.ValidationPattern))
                        throw new Exception("UploadValidator was invalid for this bank account.");
                }

                if (lineNo > parserConfig.HeaderRows)
                {
                    // split the file row and map it to the DTO
                    var fields = line.Split(delimiter);
                    yield return parser.MapImportRow<T>(bankAccountId, lineNo, fields);
                }
            }
        }

        protected string ParseValidator(string line, string validationPattern)
        {
            var regex = new Regex(validationPattern);
            var match = regex.Match(line.Replace("\"", ""));
            return match.Groups[1].Value;
        }

    }

    public class ParserConfig
    {
        public int Id { get; set; }
        public int BankAccountId { get; set; }
        public char Delimiter { get; set; }
        public int HeaderRows { get; set; }
        public bool IsSortDesc { get; set; }
        public string ParserName { get; set; }
        public string ValidationPattern { get; set; }
        public int ValidationRowNo { get; set; }
        public string ValidationValue { get; set; }
    }

    public interface IParser
    {
        T MapImportRow<T>(int bankAccountId, int rowNo, string[] data);
    }

    public class FirstCommunityParser : IParser
    {
        private StringBuilder stringBuilder = new StringBuilder();

        public T MapImportRow<T>(int bankAccountId, int rowNo, string[] data)
        {
            var checkNo = GetValue(data[7]);

            var item = new
            {
                Amount = double.Parse(GetValue(data[4], data[5], data[8])),
                Balance = double.Parse(GetValue(data[6])),
                BankAccountId = bankAccountId,
                CheckNo = string.IsNullOrWhiteSpace(checkNo) ? new int?() : int.Parse(checkNo),
                RowNo = rowNo,
                TransactionDate = DateTime.Parse(GetValue(data[1])),
                TransactionDesc = GetDescription(GetValue(data[2]), GetValue(data[3])),
                TransactionNo = GetValue(data[0])
            };

            return Mapper.Map<T>(item);
        }

        private string GetDescription(string string1, string string2)
        {
            Match match = Regex.Match(string1, "^(\\d+\\s*)+");

            if (match.Success)
            {
                stringBuilder.Clear();
                int matchLen = match.Value.Length;
                stringBuilder.Append(string1.Substring(matchLen, string1.Length - matchLen));
                stringBuilder.Append(" ");
                stringBuilder.Append(string2);
                stringBuilder.Append(" (");
                stringBuilder.Append(match.Value.Trim());
                stringBuilder.Append(")");
                return stringBuilder.ToString();
            }
            else
            {
                return string.Concat(string1, " ", string2);
            }
        }

        private string GetValue(params string[] items)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item)) return item.Replace("\"", "");
            }

            return "";
        }
    }

    public enum Parsers
    {
        FirstCommunity
    }

    public static class ParserFactory
    {
        private static Dictionary<Parsers, Func<IParser>> parsers = new Dictionary<Parsers, Func<IParser>>
        {
            { Parsers.FirstCommunity, () => new FirstCommunityParser() }
        };

        public static IParser GetParser(Parsers parserKind)
        {
            return parsers[parserKind].Invoke();
        }

        public static IParser GetParser(string parserName)
        {
            var parserKind = (Parsers)Enum.Parse(typeof(Parsers), parserName);
            return GetParser(parserKind);
        }
    }

}
