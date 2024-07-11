using System;
using SlotAppointment.Dtos;

namespace SlotAppointment.Helpers
{
    internal static class AppointmentHelper
	{
        internal static Appointment MapAppointmentRequestToAppointment(AppointmentRequest appointmentRequest)
        {
            return new Appointment
            {
                FacilityId = appointmentRequest.FacilityId,
                Start = appointmentRequest.Start.ToString("yyyy-MM-dd HH:mm:ss"),
                End = appointmentRequest.End.ToString("yyyy-MM-dd HH:mm:ss"),
                Patient = appointmentRequest.Patient,
                Comments = appointmentRequest.Comments
            };
        }
    }
}

