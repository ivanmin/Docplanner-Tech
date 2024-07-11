# Docplanner-Tech
Docplanner Tech
Instructions for use:

The project is developed in NET 7.

Just start the application and the UI (Swagger) will open.

I have two endpoints.

- GET: GetWeeklyFreeSlots:
Input parameter: desiredDate (DateTime format, example "20240829T10:00:00").
Usage: The frontend sends us a certain date, this cannot be less than the current date or greater than 8 months from the current time (I have this value in the MaxMonthsToReserve value of appsettings, and therefore could be modified. This value will be equivalent to the maximum number of months that we will allow to obtain appointments).
Upon receiving the date, the backend must obtain all the available appointments in the week to which that date belongs. If the desired day is not Monday, the backend will obtain the date of Monday of that week to pass it to the slots service.
Once these free slots are obtained, the output format will be the following:
{"facilityId": "7960f43f-67dc-4bc5-a52b-9a3494306749",
"freeSlots": [
{
"start": "2024-07-29T09:10:00",
"end": "2024-07-29T09:20:00"
},
{
"start": "2024-07-29T09:20:00",
"end": "2024-07-29T09:30:00"
},
{
"start": "2024-07-29T10:00:00",
"end": "2024-07-29T10:10:00"
},...
]}
The frontend I would need to know the time of that slot (start and end) and the facilityId that this week is associated with (this value was not in the instructions but the time slot service required that value in the POST method, so the frontend needs to know it and I add it to the response).

- POST: TakeSlotByUser:
Input parameter:
{
"facilityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
"start": "2024-07-09T10:03:46.859Z",
"end": "2024-07-09T10:03:46.859Z",
"comments": "string",
"patient": {
"name": "string",
"secondName": "string",
"email": "string",
"phone": "string"
}
}
Usage: facilityId is a GUID, start and end are DateTimes, and the rest are strings. The only one I've decided not to be mandatory from the strings is comments.
This data will be used to consume the POST from the slots service.

Remarks:
- I've used a fairly simple layered architecture. It could be refactored to a clean architecture if it were a larger project, but in this case I think it would overload the project.
- In our structure, we have a controller, a service (where the logic is), a remote access to the slots service, the DTOs, some Helpers for the service and some configuration classes.
- These configuration variables are in appsettings. They could have been put in a Constants class since it is a small project that is not going to be deployed, but I prefer this practice that seems cleaner to me.
- I have used HttpClientFactory to access the remote slots service. As in the previous point, it is a small project and it will not have a big impact on the final result, but since it ensures the reuse of connections and is not complex, I think it is a good idea to use it.
- The use of interfaces for dependency injection also seems to me a good practice as a general rule.
- The slots service returned an error if I did not send the FacilityId in the POST. So I've added it (also in the GET since the frontend will need to know that value).
- The SLOT service doesn't do some validations (I can save slots with wrong times, it doesn't check the facilityId...). Integration tests would be more suitable to check these problems together, so I don't have it addressed in the unit tests. I also haven't solved in my application the fact that non-existent slots can be reserved, for example, since I would have to make calls to the GET method before calling the POST and do more complex operations than the slot service would have to do, which is the one that accesses the database and should do those checks.
- I've done unit tests for the controller, service and external service, trying to do checks with correct data, exceptions, null data...
