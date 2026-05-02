using MessageHawk.Application.Options;
using MessageHawk.Infrastructure;
using MessageHawk.Worker;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));
builder.Services.AddMessageHawkInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ShardIngestConsumerHostedService>();

if (OperatingSystem.IsWindows() && builder.Configuration.GetValue("Worker:UseWindowsService", false))
    builder.Services.AddWindowsService(o => o.ServiceName = "MessageHawk Ingest Worker");

var host = builder.Build();
host.Run();
