using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("ShoppingCartItems")]
    public class ShoppingCartItem
    {
        [Key]
        public int CartItemID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Cantidad { get; set; } = 1;

        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        // Relaciones
        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Producto { get; set; }
    }
}