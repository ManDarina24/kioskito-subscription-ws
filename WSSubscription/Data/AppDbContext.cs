using Microsoft.EntityFrameworkCore;
using WSSuscripcion.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WSSuscripcion.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Definimos las tablas
        //public DbSet<SuscriptionsEntity> Suscriptions { get; set; }
    }
}
