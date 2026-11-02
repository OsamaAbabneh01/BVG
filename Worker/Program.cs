using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Workers;


var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddOptions<BackgroundJobOptions>()
    .Bind(builder.Configuration.GetSection(
        BackgroundJobOptions.SectionName))
    .Validate(options => options.MaxAttempts > 0,
        "MaxAttempts must be greater than zero.")
    .Validate(options => options.LeaseSeconds > 0,
        "LeaseSeconds must be greater than zero.")
    .Validate(options => options.MaxConcurrentJobs > 0,
        "MaxConcurrentJobs must be greater than zero.")
    .Validate(options => options.BulkSize > 0,
        "BulkSize must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddScoped<BackgroundJobHeartbeat>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<BackgroundJobClaimer>();
builder.Services.AddScoped<BackgroundJobProcessor>();

var host = builder.Build();
host.Run();
