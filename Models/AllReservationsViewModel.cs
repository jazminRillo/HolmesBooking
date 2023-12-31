﻿
using System.ComponentModel.DataAnnotations.Schema;
using HolmesBooking;

public class AllReservationsViewModel
{
    public ICollection<Reservation>? Reservations { get; set; }
    public ICollection<Service>? Services { get; set; }
    public DateTime? SelectedDate { get; set; }
    public ICollection<Guid>? SelectedServices { get; set; }
    [NotMapped]
    public int TotalNumberDiners { get; set; }
    public bool? IsOnline { get; set; }
}
