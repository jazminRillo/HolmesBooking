namespace HolmesBooking;

public enum State
{
    PRESENTE,
    CONFIRMADA,
    DEMORADA,
    NOSHOW,
    CANCELADA
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        switch (enumValue)
        {
            case State.PRESENTE:
                return "Ha llegado";
            case State.CONFIRMADA:
                return "Confirmada";
            case State.CANCELADA:
                return "Cancelada";
            case State.DEMORADA:
                return "Demorada";
            case State.NOSHOW:
                return "No Show";
            default:
                return "Sin confirmar";
        }
    }
}