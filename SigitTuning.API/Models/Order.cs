using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estatus { get; set; } = "Pendiente";

        [Required]
        [MaxLength(500)]
        public string DireccionEnvio { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(100)]
        public string? Estado { get; set; }

        [MaxLength(20)]
        public string? CodigoPostal { get; set; }

        [MaxLength(20)]
        public string? TelefonoContacto { get; set; }

        [MaxLength(100)]
        public string? NumeroSeguimiento { get; set; }

        public DateTime? FechaEnvio { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [MaxLength(50)]
        public string? MetodoPago { get; set; }

        // Relaciones
        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }

        public virtual ICollection<OrderDetail>? Detalles { get; set; }
    }
}