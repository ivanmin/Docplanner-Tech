using SlotAppointment.Dtos;

namespace SlotAppointment.Services
{
    public interface ISlotAppointmentService
	{
        Task<ScheduleResponse> GetWeeklyFreeSlots(DateTime desiredDate);
        Task TakeAppointmentByUser(AppointmentRequest appointment);
    }
}

