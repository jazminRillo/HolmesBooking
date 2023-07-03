using System;

namespace HolmesBooking
{
    public static class ScheduleTime
    {
        public static List<TimeSpan> GetAvailableTimes()
        {
            return new List<TimeSpan>
            {
                TimeSpan.Parse("13:00"),
                TimeSpan.Parse("13:30"),
                TimeSpan.Parse("14:00"),
                TimeSpan.Parse("14:30"),
                TimeSpan.Parse("15:00"),
                TimeSpan.Parse("15:30"),
                TimeSpan.Parse("16:00"),
                TimeSpan.Parse("16:30"),
                TimeSpan.Parse("19:00"),
                TimeSpan.Parse("19:30"),
                TimeSpan.Parse("20:00"),
                TimeSpan.Parse("20:30"),
                TimeSpan.Parse("21:00"),
                TimeSpan.Parse("21:30"),
            };
        }

    }

}
