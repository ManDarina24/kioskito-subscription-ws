using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace WSSubscription.Entities
{

    [Table("Suscriptions")] 
    public class Suscription
    {
        public int Id { get; set; }

        [Column("id_user")]
        public int UserId { get; set; }

        public string Status { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("id_plan")]
        public int PlanId { get; set; }

        [Column("stripe_subscription_id")]
        public string StripeSubscriptionId { get; set; }

        [Column("stripe_invoice_id")]
        public string StripeInvoiceId { get; set; }

        // Propiedad de navegación 
        public User User { get; set; }
        public Plan Plan { get; set; }
    }
}
