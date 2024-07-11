namespace SlotAppointment.Dtos
{
	public class Schedule
    {
        public required Facility Facility { get; set; }
        public int SlotDurationMinutes { get; set; }
        public DaySchedule? Monday { get; set; }
        public DaySchedule? Tuesday { get; set; }
        public DaySchedule? Wednesday { get; set; }
        public DaySchedule? Thursday { get; set; }
        public DaySchedule? Friday { get; set; }

        public TimeSpan SlotDurationMinutesTimespan => TimeSpan.FromMinutes(SlotDurationMinutes);
    }
}