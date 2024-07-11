namespace SlotAppointment.Dtos
{
    public class WorkPeriod
    {
        public int StartHour { get; set; }
        public int LunchStartHour { get; set; }
        public int LunchEndHour { get; set; }
        public int EndHour { get; set; }

        public TimeSpan StartHourTimespan => TimeSpan.FromHours(StartHour);
        public TimeSpan LunchStartHourTimespan => TimeSpan.FromHours(LunchStartHour);
        public TimeSpan LunchEndHourTimespan => TimeSpan.FromHours(LunchEndHour);
        public TimeSpan EndHourTimespan => TimeSpan.FromHours(EndHour);
    }
}

