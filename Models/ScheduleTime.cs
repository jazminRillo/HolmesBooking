namespace HolmesBooking;

public class ScheduleTime
{
    public int? Hour { get; set; }
    public int? Minute { get; set; }

    public ScheduleTime()
    {
    }
    
    public ScheduleTime(int Hour, int Minute)
    {
        this.Hour = Hour;
        this.Minute = Minute;
    }

    public override string ToString()
    {
        return $"{Hour:D2}:{Minute:D2}";
    }
}
