namespace HolmesBooking;

public class ServiceMocks
{
    public List<ScheduleTime> DayTime { get; set; }
    public List<ScheduleTime> NightTime { get; set; }
    public Schedule DaySchedule { get; set; }
    public Schedule NightSchedule { get; set; }
    public List<Service> AvailableServices { get; set; }
    Guid Visita_Guiada_randomId = Guid.NewGuid();
    Guid Visita_Guiada_Almuerzo_randomId = Guid.NewGuid();
    Guid Almuerzo_randomId = Guid.NewGuid();
    Guid Cena_randomId = Guid.NewGuid();

    public ServiceMocks()
    {
        this.DayTime = new List<ScheduleTime>()
        {
            new ScheduleTime(13, 00),
            new ScheduleTime(13, 30),
            new ScheduleTime(14, 00),
            new ScheduleTime(14, 30),
            new ScheduleTime(15, 00),
            new ScheduleTime(15, 30),
            new ScheduleTime(16, 00),

        };

        this.NightTime = new List<ScheduleTime>()
        {
            new ScheduleTime(19, 00),
            new ScheduleTime(19, 30),
            new ScheduleTime(20, 00),
            new ScheduleTime(20, 30),
            new ScheduleTime(21, 00),
            new ScheduleTime(21, 30),
            new ScheduleTime(22, 00),
        };

        this.DaySchedule = new Schedule();
        DaySchedule.AddSchedule(DayOfWeek.sábado, DayTime);
        DaySchedule.AddSchedule(DayOfWeek.domingo, DayTime);

        this.NightSchedule = new Schedule();
        NightSchedule.AddSchedule(DayOfWeek.viernes, NightTime);
        NightSchedule.AddSchedule(DayOfWeek.sábado, NightTime);
        NightSchedule.AddSchedule(DayOfWeek.domingo, NightTime);

        Service Visita_Guiada = new Service(
            Visita_Guiada_randomId,
            "Visita Guiada",
            new DateTime(2023, 1, 1),
            new DateTime(2023, 12, 1),
            true,
            5,
            DaySchedule
        );

        Service Visita_Guiada_Almuerzo = new Service(
            Visita_Guiada_Almuerzo_randomId,
            "Visita Guiada con Almuerzo",
            new DateTime(2023, 1, 1),
            new DateTime(2023, 12, 1),
            true,
            5,
            DaySchedule
        );

        Service Almuerzo = new Service(
           Almuerzo_randomId,
           "Almuerzo",
           new DateTime(2023, 3, 1),
           new DateTime(2023, 9, 1),
           true,
           8,
           DaySchedule
        );

        Service Cena = new Service(
           Cena_randomId,
           "Cena",
           new DateTime(2023, 9, 1),
           new DateTime(2024, 3, 1),
           true,
           8,
           NightSchedule
        );

        this.AvailableServices = new List<Service>(){
            Visita_Guiada,
            Visita_Guiada_Almuerzo,
            Almuerzo,
            Cena
        };
    }
}