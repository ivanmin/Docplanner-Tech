using SlotAppointment.Dtos;

namespace SlotAppointment.ExternalServices
{
    public interface ISlotExternalService
	{
        Task<Schedule?> GetWeeklyAvailability(string date);
        Task<bool> TakeSlot(Appointment appointment);
    }
}