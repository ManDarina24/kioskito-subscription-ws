using System.ComponentModel.DataAnnotations.Schema;

namespace WSSubscription.Entities
{
    [Table("Users")] 
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [Column("stripe_customer_id")]
        public string? StripeCustomerId { get; set; }
        public ICollection<Suscription> Suscriptions { get; set; }


    }
}
