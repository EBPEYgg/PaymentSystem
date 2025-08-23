using NLog;
using NLog.Web;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Services;
using PaymentSystem.Web.Extensions;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Initializing application");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ICartsService, CartsService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IMerchantsService, MerchantsService>();

builder
    .AddBearerAuthentication()
    .AddOptions()
    .AddSwagger()
    .AddData();

var app = builder.Build();
logger.Info("Starting the app");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();