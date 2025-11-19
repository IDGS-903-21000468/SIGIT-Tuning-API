using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("MarketplaceBids")]
    public class MarketplaceBid
    {
        [Key]
        public int BidID { get; set; }

        [Required]
        public int ListingID { get; set; }

        [Required]
        public int UserID_Comprador { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoOferta { get; set; }

        public DateTime FechaOferta { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Mensaje { get; set; }

        public bool Aceptada { get; set; } = false;

        // Relaciones
        [ForeignKey("ListingID")]
        public virtual MarketplaceListing? Publicacion { get; set; }

        [ForeignKey("UserID_Comprador")]
        public virtual User? Comprador { get; set; }
    }
}