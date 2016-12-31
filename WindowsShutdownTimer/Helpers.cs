using System;

namespace WindowsShutdownTimer
{
    public class Helpers
    {
        /// <summary>
        /// Since VS won't save a default DateTime object (year as 0001), an extra year needs to be added to
        /// any DateTime object. So I redefine the 'default' DateTime (year is now 0002) for reference.
        /// </summary>
        /// <param name="obj">DateTime object to be reset to default. Exact same as calling default except with an extra year.</param>
        public void SetDefaultDateTime(DateTime obj)
        {
            obj = default(DateTime);
            obj.AddYears(1);
        }
    }
}
