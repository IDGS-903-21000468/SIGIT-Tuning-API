using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== AGREGAR AL CARRITO =====
    public class AddToCartDto
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }
    }

    // ===== ITEM DEL CARRITO =====
    public class CartItemDto
    {
        public int CartItemID { get; set; }
        public int ProductID { get; set; }
        public string ProductoNombre { get; set; }
        public string? ProductoImagen { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
        public int StockDisponible { get; set; }
    }

    // ===== CARRITO COMPLETO =====
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Cantidad);
    }

    // ===== CREAR PEDIDO =====
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "La dirección de envío es requerida")]
        [MaxLength(500)]
        public string DireccionEnvio { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(100)]
        public string? Estado { get; set; }

        [MaxLength(20)]
        public string? CodigoPostal { get; set; }

        [Required(ErrorMessage = "El teléfono de contacto es requerido")]
        [MaxLength(20)]
        public string TelefonoContacto { get; set; }

        [MaxLength(50)]
        public string? MetodoPago { get; set; }
    }

    // ===== PEDIDO =====
    public class OrderDto
    {
        public int OrderID { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; }
        public string DireccionEnvio { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? NumeroSeguimiento { get; set; }
        public List<OrderDetailDto> Detalles { get; set; } = new();
    }

    // ===== DETALLE DEL PEDIDO =====
    public class OrderDetailDto
    {
        public int ProductID { get; set; }
        public string ProductoNombre { get; set; }
        public string? ProductoImagen { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    // ===== ACTUALIZAR ESTATUS DEL PEDIDO (ADMIN) =====
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Estatus { get; set; } // "Pendiente", "Empacando", "Enviado", "En Tránsito", "Entregado"

        public string? NumeroSeguimiento { get; set; }
    }
}