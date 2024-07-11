namespace SlotAppointment.Dtos
{
    public class Facility
    {
        public required Guid FacilityId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
    }
}