using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using WSSubscription.Hubs;
using WSSubscription.Services;
using WSSubscription.UserIdProviders;
using WSSuscripcion.Data;
using WSSuscripcion.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

// Leer clave de appsettings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Configurar Stripe globalmente
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Agregar servicios como controladores, Swagger, etc.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Mis servicios 
builder.Services.AddScoped<WSSubscription.Services.ISubscriptionService, WSSubscription.Services.SubscriptionService>();
builder.Services.AddScoped<WSSubscription.Services.ICancelSubscriptionService, WSSubscription.Services.CancelSubscriptionService>();
builder.Services.AddScoped<ISubscriptionWebhookService, SubscriptionWebhookService>();
builder.Services.AddSingleton<IUserIdProvider, SubUserIdProvider>();


//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Ahora construimos la aplicación
var app = builder.Build();

// Configurar los middlewares (por ejemplo, routing)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");

app.UseCors("AllowFrontend");
app.Run();
