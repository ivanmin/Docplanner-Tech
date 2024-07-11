using System;
using Microsoft.Extensions.Options;
using SlotAppointment.Dtos;
using SlotAppointment.ExternalServices;
using SlotAppointment.Helpers;

namespace SlotAppointment.Services
{
    public class SlotAppointmentService : ISlotAppointmentService
    {
        private readonly ILogger<ISlotAppointmentService> _logger;
        private readonly ISlotExternalService _slotServiceRemote;
        private readonly AppointmentSettings _appointmentSettings;

        public SlotAppointmentService(ILogger<ISlotAppointmentService> logger, ISlotExternalService slotServiceRemote, IOptions<AppointmentSettings> appointmentSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _slotServiceRemote = slotServiceRemote ?? throw new ArgumentNullException(nameof(slotServiceRemote));
            _appointmentSettings = appointmentSettings.Value ?? throw new ArgumentNullException(nameof(appointmentSettings));
        }

        public async Task<ScheduleResponse> GetWeeklyFreeSlots(DateTime desiredDate)
        {
            try
            {
                if (desiredDate < DateTime.Now ||
                    desiredDate > DateTime.Now.AddMonths(_appointmentSettings.MaxMonthsForAnAppointment))
                {
                    throw new ArgumentOutOfRangeException(nameof(desiredDate));
                }

                DateTime mondayOfWeek = desiredDate.AddDays(-(int)desiredDate.DayOfWeek + 1);
                var schedule = await _slotServiceRemote.GetWeeklyAvailability(mondayOfWeek.ToString("yyyyMMdd"));

                if (schedule == null)
                {
                    throw new Exception("Appointments not found");
                }

                return ScheduleHelper.GetWeeklyFreeSlotsFromSchedule(schedule, mondayOfWeek);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while processing request from GetWeeklyFreeSlots");
                throw;
            }
        }

        public async Task TakeAppointmentByUser(AppointmentRequest appointmentRequest)
        {
            try
            {
                if (appointmentRequest == null)
                {
                    throw new Exception("Cannot get appointment data");
                }

                var appointment = AppointmentHelper.MapAppointmentRequestToAppointment(appointmentRequest);               
                await _slotServiceRemote.TakeSlot(appointment);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while processing request from TakeSlotByUser");
                throw;
            }
        }
    }
}

