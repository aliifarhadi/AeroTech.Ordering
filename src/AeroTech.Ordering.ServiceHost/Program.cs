using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Ordering.ServiceHost.Composition;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddOrderingHost(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UsePresentation();

app.Run();

public partial class Program
{
}
