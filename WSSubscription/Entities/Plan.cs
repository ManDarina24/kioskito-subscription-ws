using System.ComponentModel.DataAnnotations.Schema;

namespace WSSubscription.Entities
{
    [Table("Plans")]
    public class Plan
    {
        public int Id { get; set; }
        [Column("id_price")]
        public string IdPriceStripe { get; set; }
    }
}
