using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsShutdownTimer
{
    public class Helpers
    {
        public void SetDefaultDateTime(DateTime obj)
        {
            obj = default(DateTime);
            obj.AddYears(1);
        }
    }
}
