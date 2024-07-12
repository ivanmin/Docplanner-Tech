using SlotAppointment.Dtos;

namespace SlotAppointment.Helpers
{
    internal static class ScheduleHelper
    {
        internal static ScheduleResponse GetWeeklyFreeSlotsFromSchedule(Schedule schedule, DateTime firstDayOfTheWeek)
        {
            var daySchedules = MapScheduleToDayScheduleList(schedule);
            List<DateTime> weeklyFreeSlots = new();

            for (int dayOfWeek = 0; dayOfWeek < daySchedules.Count; dayOfWeek++)
            {
                var daySchedule = daySchedules[dayOfWeek];
                if (daySchedule == null) continue;

                var dailyBusySlots = daySchedule.BusySlots?.Select(x => x.Start.TimeOfDay).ToHashSet();
                List<DateTime> dailyAllSlots = GetDailyAllSlots(firstDayOfTheWeek, dayOfWeek, daySchedule.WorkPeriod, schedule.SlotDurationMinutesTimespan);
                List<DateTime> dailyAllFutureSlots = dailyAllSlots.Where(date => date >= DateTime.Now).ToList();

                if (dailyBusySlots != null)
                {
                    weeklyFreeSlots.AddRange(dailyAllFutureSlots.Where(slot => !dailyBusySlots.Contains(slot.TimeOfDay)));
                }
                else
                {
                    weeklyFreeSlots.AddRange(dailyAllFutureSlots);
                }
            }

            return new ScheduleResponse
            {
                FacilityId = schedule.Facility.FacilityId,
                FreeSlots = MapListOfDateTimesToSlotsList(weeklyFreeSlots, schedule.SlotDurationMinutesTimespan)
            };
        }

        internal static List<DateTime> GetDailyAllSlots(DateTime firstDayOfTheWeek, int dayOfWeek, WorkPeriod workPeriod, TimeSpan slotDurationMinutes)
        {
            List<DateTime> response = new();
            DateTime currentDay = firstDayOfTheWeek.Date.AddDays(dayOfWeek);
            for (TimeSpan time = workPeriod.StartHourTimespan; time < workPeriod.LunchEndHourTimespan; time += slotDurationMinutes)
            {
                if (time <= workPeriod.LunchStartHourTimespan || time > workPeriod.LunchEndHourTimespan)
                {
                    DateTime slot = currentDay.Add(time);
                    response.Add(slot);
                }
            }

            return response;
        }

        internal static List<DaySchedule?> MapScheduleToDayScheduleList(Schedule schedule)
        {
            return new List<DaySchedule?>
            {
                schedule.Monday,
                schedule.Tuesday,
                schedule.Wednesday,
                schedule.Thursday,
                schedule.Friday
            };
        }

        internal static List<Slot> MapListOfDateTimesToSlotsList(List<DateTime> slots, TimeSpan slotDuration)
        {
            return slots.Select(date => new Slot
            {
                Start = date,
                End = date.Add(slotDuration)
            }).ToList();
        }
    }
}

