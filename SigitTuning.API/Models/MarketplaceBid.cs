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

        // ✅ CORREGIDO: Este campo es necesario para que no te marque error en el Controller
        // (Coincide con la columna BIT que agregamos en SQL)
        public bool Aceptada { get; set; } = false;

        // 🆕 NUEVOS CAMPOS (Coinciden con NVARCHAR y DATETIME en SQL)
        [Required]
        [MaxLength(20)]
        public string Estatus { get; set; } = "Pendiente"; // "Pendiente", "Aceptada", "Rechazada"

        public DateTime? FechaRespuesta { get; set; }

        // --- RELACIONES ---
        [ForeignKey("ListingID")]
        public virtual MarketplaceListing? Publicacion { get; set; }

        [ForeignKey("UserID_Comprador")]
        public virtual User? Comprador { get; set; }
    }
}