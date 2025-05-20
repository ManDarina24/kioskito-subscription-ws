using Microsoft.EntityFrameworkCore;
using Stripe;
using WSSubscription.Services;
using WSSuscripcion.Data;
using WSSuscripcion.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080"; // Usa 8080 como fallback

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configurar DbContext con MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

// Leer clave de appsettings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Configurar Stripe globalmente
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Agregar servicios como controladores, Swagger, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Mis servicios 
builder.Services.AddScoped<WSSubscription.Services.ISubscriptionService, WSSubscription.Services.SubscriptionService>();
builder.Services.AddScoped<WSSubscription.Services.ICancelSubscriptionService, WSSubscription.Services.CancelSubscriptionService>();
builder.Services.AddScoped<ISubscriptionWebhookService, SubscriptionWebhookService>();



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
app.UseCors("AllowFrontend");
app.Run();
