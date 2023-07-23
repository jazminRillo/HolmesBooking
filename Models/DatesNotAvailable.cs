using System.ComponentModel.DataAnnotations.Schema;

namespace HolmesBooking
{
    [Table("DatesNotAvailable")]
    public class DatesNotAvailable
    {
        public Guid? Id { get; set; }
        public DateTime Date { get; set; }
    }
}

