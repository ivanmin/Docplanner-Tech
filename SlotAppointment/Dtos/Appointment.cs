namespace SlotAppointment.Dtos
{
	public class Appointment
	{
        public required Guid FacilityId { get; set; }
        public required string Start { get; set; }
        public required string End { get; set; }
        public string Comments { get; set; } = string.Empty;
        public required Patient Patient { get; set; }
    }
}