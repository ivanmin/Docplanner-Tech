using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SlotAppointment.Dtos;
using SlotAppointment.ExternalServices;

namespace SlotAppointment.UnitTests
{
    public class SlotExternalServiceTests
    {
        private readonly SlotExternalService _externalService;
        private readonly Mock<ILogger<ISlotExternalService>> _mockLogger;
        private readonly IOptions<SlotServiceSettings> _mockConfiguration;
        private readonly Mock<IHttpClientFactory> _mockClientFactory;
        private readonly Appointment _appointment;
        private readonly Schedule _schedule;

        public SlotExternalServiceTests()
        {
            _mockLogger = new Mock<ILogger<ISlotExternalService>>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            var configValues = new SlotServiceSettings
            {
                Username = "techuser",
                Password = "secretpassWord",
                BaseUrl = "https://draliatest.azurewebsites.net/api/availability"
            };
            _mockConfiguration = Options.Create(configValues);

            _externalService = new SlotExternalService(_mockLogger.Object, _mockClientFactory.Object, _mockConfiguration);

            _appointment = new Appointment()
            {
                FacilityId = new Guid(),
                Comments = "",
                End = "2024-07-31 12:00:00",
                Start = "2024-07-31 12:10:00",
                Patient = new Patient()
                {
                    Name = "test name",
                    SecondName = "test secondname",
                    Email = "test@test.com",
                    Phone = "555000000"
                }
            };

            _schedule = new Schedule
            {
                Facility = new Facility()
                {
                    FacilityId = new Guid(),
                    Address = "test",
                    Name = "test",
                },
                SlotDurationMinutes = 10,
                Monday = new DaySchedule()
                {
                    WorkPeriod = new WorkPeriod()
                    {
                        StartHour = 10,
                        LunchStartHour = 14,
                        LunchEndHour = 15,
                        EndHour = 18
                    },
                    BusySlots = new List<Slot>()
                    {
                        new Slot()
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

        private static HttpClient CreateHttpClient(HttpStatusCode httpStatusCode, string content)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = httpStatusCode,
                       Content = new StringContent(content),
                   });

            return new HttpClient(handler.Object);
        }

        [Fact]
        public async Task GetWeeklyAvailability_InternalServerErrorFromServer_ThrowsNull()
        {
            // Arrange
            var date = "2024-07-15";
            var errorMessage = "test error message";
            var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, errorMessage);
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var response = await _externalService.GetWeeklyAvailability(date);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task GetWeeklyAvailability_EmptyResponseFromServer_ThrowsNull()
        {
            // Arrange
            var date = "2024-07-15";
            var httpClient = CreateHttpClient(HttpStatusCode.OK, "{}");
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var response = await _externalService.GetWeeklyAvailability(date);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task GetAvailableSlots_OkResponseFromServer_ReturnsSuccess()
        {
            // Arrange
            var date = "2024-07-15";
            var expectedSchedule = _schedule;

            var httpClient = CreateHttpClient(HttpStatusCode.OK, JsonConvert.SerializeObject(expectedSchedule));
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var schedule = await _externalService.GetWeeklyAvailability(date);

            // Assert
            Assert.NotNull(schedule);
            Assert.Equal(expectedSchedule.Facility.FacilityId, schedule.Facility.FacilityId);
            Assert.Equal(expectedSchedule.Facility.Name, schedule.Facility.Name);
            Assert.Equal(expectedSchedule.Facility.Address, schedule.Facility.Address);
            Assert.Equal(expectedSchedule.SlotDurationMinutes, schedule.SlotDurationMinutes);
            Assert.Equal(expectedSchedule.SlotDurationMinutesTimespan, schedule.SlotDurationMinutesTimespan);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.StartHour, schedule.Monday?.WorkPeriod.StartHour);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.LunchStartHour, schedule.Monday?.WorkPeriod.LunchStartHour);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.LunchEndHourTimespan, schedule.Monday?.WorkPeriod.LunchEndHourTimespan);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.EndHour, schedule.Monday?.WorkPeriod.EndHour);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.StartHourTimespan, schedule.Monday?.WorkPeriod.StartHourTimespan);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.LunchStartHourTimespan, schedule.Monday?.WorkPeriod.LunchStartHourTimespan);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.LunchEndHourTimespan, schedule.Monday?.WorkPeriod.LunchEndHourTimespan);
            Assert.Equal(expectedSchedule.Monday?.WorkPeriod.EndHourTimespan, schedule.Monday?.WorkPeriod.EndHourTimespan);
            Assert.Equal(expectedSchedule.Monday?.BusySlots?[0].Start, schedule.Monday?.BusySlots?[0].Start);
            Assert.Equal(expectedSchedule.Monday?.BusySlots?[0].End, schedule.Monday?.BusySlots?[0].End);
            Assert.Equal(expectedSchedule.Tuesday, schedule.Tuesday);
            Assert.Equal(expectedSchedule.Wednesday, schedule.Wednesday);
            Assert.Equal(expectedSchedule.Thursday, schedule.Thursday);
            Assert.Equal(expectedSchedule.Friday, schedule.Friday);
        }

        [Fact]
        public async Task TakeSlot_BadRequestFromServer_ReturnsFalse()
        {
            // Arrange
            var appointment = _appointment;
            var httpClient = CreateHttpClient(HttpStatusCode.BadRequest, "{\"error\": \"slot not available\"}");
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var confirmation = await _externalService.TakeSlot(appointment);

            // Assert
            Assert.False(confirmation);
        }

        [Fact]
        public async Task TakeSlot_InternalServerErrorFromServer_ReturnsFalse()
        {
            // Arrange
            var appointment = _appointment;
            var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "");
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var confirmation = await _externalService.TakeSlot(appointment);

            // Assert
            Assert.False(confirmation);
        }

        [Fact]
        public async Task TakeSlot_OkResponseFromServer_ReturnsSuccess()
        {
            // Arrange
            var appointment = _appointment;
            var httpClient = CreateHttpClient(HttpStatusCode.OK, "true");
            _mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var confirmation = await _externalService.TakeSlot(appointment);

            // Assert
            Assert.True(confirmation);
        }

    }
}

