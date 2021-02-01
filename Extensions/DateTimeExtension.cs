using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Extensions
{
    static public class DateTimeExtension
    {
        static public bool IsUTC(this DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }
    }
}
