namespace SlotAppointment.Dtos
{
    public class DaySchedule
    {
        public required WorkPeriod WorkPeriod { get; set; }
        public List<Slot>? BusySlots { get; set; }
    }
}

