using Fcg.Payments.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServicesExtension();
var app = builder.Build();
app.UseSerilogRequestLogging();
app.MapHealthCheckEndpointExtension();
app.Run();
