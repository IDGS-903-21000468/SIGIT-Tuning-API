using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("ProductCategories")]
    public class ProductCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relación: Una categoría tiene muchos productos
        public virtual ICollection<Product>? Productos { get; set; }
    }
}