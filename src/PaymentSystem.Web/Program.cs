using NLog;
using NLog.Web;
using PaymentSystem.Extensions;
using PaymentSystem.Web.Extensions;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Initializing application");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder
    .AddBearerAuthentication()
    .AddOptions()
    .AddSwagger()
    .AddData()
    .AddApplicationServices();

var app = builder.Build();
logger.Info("Starting the app");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseErrorHandling();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();