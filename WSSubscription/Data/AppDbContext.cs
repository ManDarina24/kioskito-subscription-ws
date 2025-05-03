using Microsoft.EntityFrameworkCore;
using WSSuscripcion.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using WSSubscription.Entities;

namespace WSSuscripcion.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Definimos las tablas
        public DbSet<User> Users { get; set; }
        public DbSet<Suscription> Suscriptions { get; set; }
        public DbSet<Plan> Plans { get; set; }




    }
}
