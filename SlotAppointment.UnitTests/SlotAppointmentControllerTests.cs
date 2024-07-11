using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SlotAppointment.Controllers;
using SlotAppointment.Dtos;
using SlotAppointment.Services;

namespace SlotAppointment.UnitTests
{
    public class SlotAppointmentControllerTests
    {
        private readonly SlotAppointmentController _controller;
        private readonly Mock<ISlotAppointmentService> _mockSlotAppointmentService;
        private readonly Mock<ILogger<SlotAppointmentController>> _mockLogger;
        private readonly IOptions<AppointmentSettings> _mockConfiguration;
        private readonly AppointmentRequest _appointmentRequest;

        public SlotAppointmentControllerTests()
        {
            _mockSlotAppointmentService = new Mock<ISlotAppointmentService>();
            _mockLogger = new Mock<ILogger<SlotAppointmentController>>();
            var configValues = new AppointmentSettings { MaxMonthsForAnAppointment = 8 };
            _mockConfiguration = Options.Create(configValues);

            _controller = new SlotAppointmentController(_mockLogger.Object, _mockSlotAppointmentService.Object, _mockConfiguration);

            _appointmentRequest = new AppointmentRequest()
            {
                Comments = "",
                FacilityId = Guid.Empty,
                End = DateTime.Today,
                Start = DateTime.Today,
                Patient = new Patient
                {
                    Name = "",
                    SecondName = "",
                    Email = "",
                    Phone = ""
                }
            };
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_DateEarlierThanNow_ReturnsBadRequest()
        {
            // Arrange
            var pastDate = DateTime.Now.AddDays(-1);

            // Act
            var result = await _controller.GetWeeklyFreeSlots(pastDate);

            // Assert
            Assert.Equal(400, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_DateLaterThanAllowed_ReturnsBadRequest()
        {
            // Arrange
            var futureDate = DateTime.Now.AddMonths(9);

            // Act
            var result = await _controller.GetWeeklyFreeSlots(futureDate);

            // Assert
            Assert.Equal(400, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_ServiceThrowsException_ReturnsProblem()
        {
            // Arrange
            var validDate = DateTime.Now.AddMonths(2);
            _mockSlotAppointmentService.Setup(service => service.GetWeeklyFreeSlots(validDate))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetWeeklyFreeSlots(validDate);

            // Assert
            Assert.Equal(500, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_ValidDate_ReturnsOk()
        {
            // Arrange
            var validDate = DateTime.Now.AddMonths(2);
            _mockSlotAppointmentService.Setup(service => service.GetWeeklyFreeSlots(validDate))
                .ReturnsAsync(new ScheduleResponse());

            // Act
            var result = await _controller.GetWeeklyFreeSlots(validDate);

            // Assert
            Assert.Equal(200, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task TakeAppointmentByUser_NullAppointmentRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.TakeAppointmentByUser(null);

            // Assert
            Assert.Equal(400, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task TakeAppointmentByUser_ServiceThrowsException_ReturnsProblem()
        {
            // Arrange
            var appointmentRequest = _appointmentRequest;
            _mockSlotAppointmentService.Setup(service => service.TakeAppointmentByUser(appointmentRequest))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.TakeAppointmentByUser(appointmentRequest);

            // Assert
            Assert.Equal(500, (result as ObjectResult)?.StatusCode);
        }

        [Fact]
        public async Task TakeAppointmentByUser_ValidAppointmentRequest_ReturnsOk()
        {
            // Arrange
            var appointmentRequest = _appointmentRequest;

            _mockSlotAppointmentService.Setup(service => service.TakeAppointmentByUser(appointmentRequest))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.TakeAppointmentByUser(appointmentRequest);

            // Assert
            Assert.IsType<OkResult>(result);
            var okResult = (OkResult)result;
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}