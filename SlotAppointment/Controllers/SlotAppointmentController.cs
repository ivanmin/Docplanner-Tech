using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlotAppointment.Dtos;
using SlotAppointment.Services;

namespace SlotAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotAppointmentController : ControllerBase
    {
        private readonly ILogger<SlotAppointmentController> _logger;
        private readonly ISlotAppointmentService _slotBookingService;
        private readonly AppointmentSettings _appointmentSettings;

        public SlotAppointmentController(ILogger<SlotAppointmentController> logger, ISlotAppointmentService slotBookingService, IOptions<AppointmentSettings> appointmentSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _slotBookingService = slotBookingService ?? throw new ArgumentNullException(nameof(slotBookingService));
            _appointmentSettings = appointmentSettings.Value ?? throw new ArgumentNullException(nameof(appointmentSettings));
        }

        [Route("GetWeeklyFreeSlots")]
        [HttpGet]
        public async Task<IActionResult> GetWeeklyFreeSlots(DateTime desiredDate)
        {
            try
            {
                if (desiredDate < DateTime.Now)
                {
                    return BadRequest(new
                    {
                        error = "InvalidDate",
                        message = "The appointment desired date cannot be earlier than the current date."
                    });
                }

                if (desiredDate > DateTime.Now.AddMonths(_appointmentSettings.MaxMonthsForAnAppointment))
                {
                    return BadRequest(new
                    {
                        error = "InvalidDate",
                        message = $"The appointment desired date cannot be later than {_appointmentSettings.MaxMonthsForAnAppointment} months."
                    });
                }

                return Ok(await _slotBookingService.GetWeeklyFreeSlots(desiredDate));
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }

        [Route("TakeSlotByUser")]
        [HttpPost]
        public async Task<IActionResult> TakeAppointmentByUser(AppointmentRequest appointmentRequest)
        {
            try
            {
                if (appointmentRequest == null)
                {
                    return BadRequest(new
                    {
                        error = "InvalidAppointment",
                        message = "The appointment data is not valid."
                    });
                }

                await _slotBookingService.TakeAppointmentByUser(appointmentRequest);
                return Ok();
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }
    }
}


