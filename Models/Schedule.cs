using System.Text;

namespace HolmesBooking;

public class Schedule
{
    public Dictionary<DayOfWeek, List<ScheduleTime>> schedule { get; set; }

    public Schedule()
    {
        schedule = new Dictionary<DayOfWeek, List<ScheduleTime>>();
    }

    public void AddSchedule(DayOfWeek DayOfWeek, List<ScheduleTime> scheduleTimes)
    {
        schedule[DayOfWeek] = scheduleTimes;
    }

    public List<ScheduleTime> GetSchedule(DayOfWeek DayOfWeek)
    {
        return schedule[DayOfWeek];
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Schedule:");

        foreach (var kvp in schedule)
        {
            sb.AppendLine($"Day: {kvp.Key}");

            foreach (var scheduleTime in kvp.Value)
            {
                sb.AppendLine($"{scheduleTime}");
            }
        }

        return sb.ToString();
    }

}
