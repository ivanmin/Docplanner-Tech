namespace SlotAppointment.Dtos
{
    public class AppointmentRequest
    {
        public required Guid FacilityId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Comments { get; set; } = string.Empty;
        public required Patient Patient { get; set; }
    }
}

