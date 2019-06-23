using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetToolsBLL.Helpers
{
    class Converters
    {
        public static double ToDouble(string data)
        {
            double dbl;
            if (!double.TryParse(data, out dbl)) return 0.0;
            return dbl;
        }
    }
}
