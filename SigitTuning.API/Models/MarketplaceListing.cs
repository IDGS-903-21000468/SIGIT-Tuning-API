using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("MarketplaceListings")]
    public class MarketplaceListing
    {
        [Key]
        public int ListingID { get; set; }

        [Required]
        public int UserID_Vendedor { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioInicial { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioActual { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        public int? Anio { get; set; }

        public int? Kilometraje { get; set; }

        public string? Modificaciones { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string Estatus { get; set; } = "Activa";

        [Column(TypeName = "decimal(5,2)")]
        public decimal ComisionPlataforma { get; set; } = 15.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioFinalVenta { get; set; }

        public DateTime? FechaVenta { get; set; }

        public int? CompradorID { get; set; }

        // Relaciones
        [ForeignKey("UserID_Vendedor")]
        public virtual User? Vendedor { get; set; }

        [ForeignKey("CompradorID")]
        public virtual User? Comprador { get; set; }

        public virtual ICollection<MarketplaceBid>? Ofertas { get; set; }
        public virtual ICollection<MarketplaceChat>? Chats { get; set; }
    }
}