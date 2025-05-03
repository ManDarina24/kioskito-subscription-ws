using Microsoft.EntityFrameworkCore;
using Stripe;
using WSSubscription.Services;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Mis servicios 
builder.Services.AddScoped<WSSubscription.Services.ISubscriptionService, WSSubscription.Services.SubscriptionService>();




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

app.Run();
