namespace SlotAppointment.Dtos
{
    public class ScheduleResponse
	{
        public Guid FacilityId { get; set; }
        public List<Slot>? FreeSlots { get; set; }
    }
}