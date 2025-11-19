using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        [MaxLength(50)]
        public string? Anio { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relaciones
        [ForeignKey("CategoryID")]
        public virtual ProductCategory? Categoria { get; set; }

        public virtual ICollection<ShoppingCartItem>? CarritoItems { get; set; }
        public virtual ICollection<OrderDetail>? DetallesPedido { get; set; }
    }
}