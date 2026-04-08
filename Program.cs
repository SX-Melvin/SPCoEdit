using NLog.Extensions.Logging;
using SPCoEdit.Configurations;
using SPCoEdit.Service;
using SPCoEdit.Utils;

var config = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
   .Build();

NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<CoEditService>();
builder.Services.AddScoped<OTCSUtils>();
builder.Services.AddScoped<SharePointUtils>();

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
