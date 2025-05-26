using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngrLink.Models
{
    public static class FeeCalculators
    {
        private static readonly Dictionary<(string year,string program), int> FeeMap = new()
    {
        { ("1st Year", "CPE"), 53000 },
        { ("2nd Year", "CPE"), 55000 },
        { ("3rd Year", "CPE"), 60000 },
        { ("4th Year", "CPE"), 68000 },

        { ("1st Year", "ARCHI"), 55000 },
        { ("2nd Year", "ARCHI"), 59000 },
        { ("3rd Year", "ARCHI"), 72000 },
        { ("4th Year", "ARCHI"), 73000 },
        { ("5th Year", "ARCHI"), 70000 },

        { ("1st Year", "CE"), 53000 },
        { ("2nd Year", "CE"), 57000 },
        { ("3rd Year", "CE"), 60000 },
        { ("4th Year", "CE"), 65000 },

        { ("1st Year", "ECE"), 53000 },
        { ("2nd Year", "ECE"), 55000 },
        { ("3rd Year", "ECE"), 60000 },
        { ("4th Year", "ECE"), 68000 }
    };

        public static int GetFee(string year, string program)
        {
            return FeeMap.TryGetValue((year, program), out var fee)
                ? fee
                : throw new ArgumentException("Fee not defined for given year and program");
        }
    }
}
