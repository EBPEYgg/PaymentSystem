using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Services;
using PaymentSystem.Web.Extensions;

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