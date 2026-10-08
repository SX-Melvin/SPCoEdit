using NLog.Extensions.Logging;
using SPCoEdit.Configurations;
using SPCoEdit.Service;
using SPCoEdit.Utils;
using SPCoEdit.Database;
using Microsoft.EntityFrameworkCore;

var config = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
   .Build();

NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? Array.Empty<string>();

    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithExposedHeaders("Content-Type", "Cache-Control", "Range")
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
));

builder.Services.AddScoped<CoEditService>();
builder.Services.AddScoped<OTCSUtils>();
builder.Services.AddScoped<DbUtils>();
builder.Services.AddScoped<SharePointUtils>();
builder.Services.AddSingleton<SharePointOnlineTokenProvider>();
builder.Services.AddScoped<CronJobService>();
builder.Services.AddOptions<CronJobConfiguration>()
    .Bind(builder.Configuration.GetSection("CronJob"))
    .Validate(options => options.IntervalMinutes > 0 && options.IntervalMinutes <= 71582,
        "CronJob:IntervalMinutes must be between 1 and 71582 minutes.")
    .Validate(options => options.IdleMinutes > 0,
        "CronJob:IdleMinutes must be greater than zero.")
    .ValidateOnStart();
builder.Services.AddHostedService<CronJobBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithExposedHeaders("Content-Type", "Cache-Control")
            .WithOrigins("http://192.168.1.198", "http://192.168.1.217", "http://agotest-vm")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.Configure<OTCSConfiguration>(builder.Configuration.GetSection("OTCS"));
builder.Services.Configure<SharePointConfiguration>(builder.Configuration.GetSection("SharePoint"));

var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
//app.UseAuthorization();
app.MapControllers();
app.Run();
