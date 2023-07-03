namespace HolmesBooking;

public class ServiceMocks
{
    public List<TimeSpan> DayTime { get; set; }
    public List<TimeSpan> NightTime { get; set; }
    public Dictionary<DayOfWeek, List<TimeSpan>> DaySchedule { get; set; }
    public Dictionary<DayOfWeek, List<TimeSpan>> NightSchedule { get; set; }
    public List<Service> AvailableServices { get; set; }
    Guid Visita_Guiada_randomId = Guid.NewGuid();
    Guid Visita_Guiada_Almuerzo_randomId = Guid.NewGuid();
    Guid Almuerzo_randomId = Guid.NewGuid();
    Guid Cena_randomId = Guid.NewGuid();

    public ServiceMocks()
    {
        DayTime = new List<TimeSpan>()
        {
            new TimeSpan(13, 00, 00),
            new TimeSpan(13, 30, 00),
            new TimeSpan(14, 00, 00),
            new TimeSpan(14, 30, 00),
            new TimeSpan(15, 00, 00),
            new TimeSpan(15, 30, 00),
            new TimeSpan(16, 00, 00),

        };

        NightTime = new List<TimeSpan>()
        {
            new TimeSpan(19, 00, 00),
            new TimeSpan(19, 30, 00),
            new TimeSpan(20, 00, 00),
            new TimeSpan(20, 30, 00),
            new TimeSpan(21, 00, 00),
            new TimeSpan(21, 30, 00),
            new TimeSpan(22, 00, 00),
        };


        DaySchedule = new Dictionary<DayOfWeek, List<TimeSpan>>
        {
            { DayOfWeek.sábado, DayTime },
            { DayOfWeek.domingo, DayTime }
        };

        NightSchedule = new Dictionary<DayOfWeek, List<TimeSpan>>
        {
            { DayOfWeek.viernes, NightTime },
            { DayOfWeek.sábado, NightTime },
            { DayOfWeek.domingo, NightTime }
        };

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

        AvailableServices = new List<Service>(){
            Visita_Guiada,
            Visita_Guiada_Almuerzo,
            Almuerzo,
            Cena
        };
    }
}