using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Shared.Utilities
{
    /// <summary>
    /// This is a set of extension methods to support converting to/from the Epoch Seconds used in JWT Tokens
    /// </summary>
    /// <remarks>
    /// the number of seconds between a particular date and time that starts at the Unix Epoch on January 1st, 1970 at UTC.
    /// </remarks>
    public static class DateTimeExtensionMethods
    {
        static DateTime EPOCH_BASE_DATETIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int ToEpochSeconds(this DateTime date)
        {
            TimeSpan t = DateTime.UtcNow - EPOCH_BASE_DATETIME;
            return (int)t.TotalSeconds;
        }

        public static DateTime FromEpochSeconds(this DateTime date, long epochSeconds)
        {
            return EPOCH_BASE_DATETIME.AddSeconds(epochSeconds);
        }
    }
}
