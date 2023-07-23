using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HolmesBooking;

public enum State
{
    [Display(Name = "Confirmada")]
    CONFIRMADA,
    [Display(Name = "Demorada")]
    DEMORADA,
    [Display(Name = "No Show")]
    NOSHOW,
    [Display(Name = "Cancelada")]
    CANCELADA
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        .Name;
    }
}