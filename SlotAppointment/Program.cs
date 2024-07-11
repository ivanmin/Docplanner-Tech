using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Models;
using SlotAppointment.Dtos;
using SlotAppointment.ExternalServices;
using SlotAppointment.Services;

[assembly: InternalsVisibleTo("SlotAppointment.UnitTests")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.Configure<AppointmentSettings>(builder.Configuration.GetSection("AppointmentSettings"));
builder.Services.Configure<SlotServiceSettings>(builder.Configuration.GetSection("SlotServiceSttings"));

builder.Services.AddScoped<ISlotAppointmentService, SlotAppointmentService>();
builder.Services.AddScoped<ISlotExternalService, SlotExternalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

