using System;

namespace Harvest.Core.Services
{
    /// <summary>
    /// This is just so the system date can be mocked out for testing
    /// </summary>
    public interface IDateTimeService
    {
        DateTime DateTimeUtcNow();
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTime DateTimeUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
