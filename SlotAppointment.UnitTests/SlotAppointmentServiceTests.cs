using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SlotAppointment.Dtos;
using SlotAppointment.ExternalServices;
using SlotAppointment.Helpers;
using SlotAppointment.Services;

namespace SlotAppointment.UnitTests
{
    public class SlotAppointmentServiceTests
    {
        private readonly SlotAppointmentService _service;
        private readonly Mock<ISlotExternalService> _mockSlotServiceRemote;
        private readonly Mock<ILogger<ISlotAppointmentService>> _mockLogger;
        private readonly IOptions<AppointmentSettings> _mockConfiguration;
        private readonly AppointmentRequest _appointmentRequest;
        private readonly Schedule _schedule;

        public SlotAppointmentServiceTests()
        {
            _mockSlotServiceRemote = new Mock<ISlotExternalService>();
            _mockLogger = new Mock<ILogger<ISlotAppointmentService>>();
            var configValues = new AppointmentSettings { MaxMonthsForAnAppointment = 8 };
            _mockConfiguration = Options.Create(configValues);

            _service = new SlotAppointmentService(_mockLogger.Object, _mockSlotServiceRemote.Object, _mockConfiguration);

            _appointmentRequest = new AppointmentRequest
            {
                FacilityId = new Guid(),
                Comments = "test comments",
                End = DateTime.ParseExact("2024-07-15 11:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                    System.Globalization.CultureInfo.InvariantCulture),
                Start = DateTime.ParseExact("2024-07-15 11:10:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                    System.Globalization.CultureInfo.InvariantCulture),
                Patient = new Patient
                {
                    Name = "test name",
                    SecondName = "test secondname",
                    Email = "test@test.com",
                    Phone = "555000000"
                }
            };

            _schedule = new Schedule
            {
                Facility = new Facility
                {
                    FacilityId = Guid.NewGuid(),
                    Address = "test address",
                    Name = "test name",
                },
                SlotDurationMinutes = 10,
                Monday = new DaySchedule
                {
                    WorkPeriod = new WorkPeriod
                    {
                        StartHour = 10,
                        LunchStartHour = 14,
                        LunchEndHour = 15,
                        EndHour = 18
                    },
                    BusySlots = new List<Slot>
                    {
                        new Slot
                        {
                            Start = DateTime.ParseExact("2024-07-15 11:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                System.Globalization.CultureInfo.InvariantCulture),
                            End = DateTime.ParseExact("2024-07-15 11:10:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                }
            };
        }
      
        [Fact]
        public async Task GetWeeklyFreeSlots_GetWeeklyAvailabilityReturnsNull_ThrowsException()
        {
            // Arrange
            var desiredDate = DateTime.Now.AddDays(7);
            _mockSlotServiceRemote.Setup(service => service.GetWeeklyAvailability(It.IsAny<string>()))
                                  .ReturnsAsync(null as Schedule);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetWeeklyFreeSlots(desiredDate));
            Assert.Equal("Appointments not found", exception.Message);
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_DateEarlierThanNow_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var pastDate = DateTime.Now.AddDays(-1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.GetWeeklyFreeSlots(pastDate));
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_DateLaterThanAllowed_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var futureDate = DateTime.Now.AddMonths(9);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.GetWeeklyFreeSlots(futureDate));
        }

        [Fact]
        public async Task GetWeeklyFreeSlots_ValidDate_ReturnsFreeSlots()
        {
            // Arrange
            var desiredDate = DateTime.Now.AddDays(7);
            var schedule = _schedule;

            _mockSlotServiceRemote.Setup(service => service.GetWeeklyAvailability(It.IsAny<string>()))
                .ReturnsAsync(schedule);

            // Act
            var result = await _service.GetWeeklyFreeSlots(desiredDate);

            // Assert
            if (result.FreeSlots == null)
            {
                result.FreeSlots = new List<Slot>();
            }

            Assert.NotNull(result);
            _mockSlotServiceRemote.Verify(service => service.GetWeeklyAvailability(It.IsAny<string>()), Times.Once);

            if (schedule.Monday?.BusySlots != null)
            {
                var busySlots = schedule.Monday.BusySlots;

                foreach (var busySlot in busySlots)
                {
                    Assert.DoesNotContain(result.FreeSlots, slot => slot.Start == busySlot.Start && slot.End == busySlot.End);
                }
            }
        }

        [Fact]
        public async Task TakeAppointmentByUser_NullAppointmentRequest_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.TakeAppointmentByUser(null));
        }

        [Fact]
        public async Task TakeAppointmentByUser_RemoteServiceThrowsException_ThrowsException()
        {
            // Arrange
            var appointmentRequest = _appointmentRequest;

            _mockSlotServiceRemote.Setup(service => service.TakeSlot(It.IsAny<Appointment>())).ThrowsAsync(new Exception("Remote service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.TakeAppointmentByUser(appointmentRequest));
            Assert.Equal("Remote service error", exception.Message);
        }

        [Fact]
        public void MapAppointmentRequestToAppointment_ValidRequest_ReturnsCorrectAppointment()
        {
            // Arrange
            var appointmentRequest = _appointmentRequest;

            // Act
            var response = AppointmentHelper.MapAppointmentRequestToAppointment(appointmentRequest);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(appointmentRequest.FacilityId, response.FacilityId);
            Assert.Equal(appointmentRequest.Comments, response.Comments);
            Assert.Equal(appointmentRequest.End.ToString("yyyy-MM-dd HH:mm:ss"), response.End);
            Assert.Equal(appointmentRequest.Start.ToString("yyyy-MM-dd HH:mm:ss"), response.Start);
            Assert.Equal(appointmentRequest.Patient.Name, response.Patient.Name);
            Assert.Equal(appointmentRequest.Patient.SecondName, response.Patient.SecondName);
            Assert.Equal(appointmentRequest.Patient.Email, response.Patient.Email);
            Assert.Equal(appointmentRequest.Patient.Phone, response.Patient.Phone);
        }

        [Fact]
        public async Task TakeAppointmentByUser_ValidAppointmentRequest_CallsTakeSlot()
        {
            // Arrange
            var appointmentRequest = _appointmentRequest;
            var appointment = AppointmentHelper.MapAppointmentRequestToAppointment(appointmentRequest);
            _mockSlotServiceRemote.Setup(service => service.TakeSlot(It.IsAny<Appointment>())).Returns(Task.FromResult(true));

            // Act
            await _service.TakeAppointmentByUser(appointmentRequest);

            // Assert
            _mockSlotServiceRemote.Verify(service => service.TakeSlot(It.Is<Appointment>(a =>
                a.FacilityId == appointment.FacilityId &&
                a.Comments == appointment.Comments &&
                a.End == appointment.End &&
                a.Start == appointment.Start &&
                a.Patient.Name == appointment.Patient.Name &&
                a.Patient.SecondName == appointment.Patient.SecondName &&
                a.Patient.Email == appointment.Patient.Email &&
                a.Patient.Phone == appointment.Patient.Phone
            )), Times.Once);
        }
    }
}


