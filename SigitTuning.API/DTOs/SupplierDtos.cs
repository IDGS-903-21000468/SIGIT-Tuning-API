using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== PROVEEDORES =====

    public class CreateSupplierDto
    {
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        [MaxLength(200)]
        public string? RazonSocial { get; set; }

        [MaxLength(20)]
        public string? RFC { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(500)]
        public string? Direccion { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(100)]
        public string? Estado { get; set; }

        [MaxLength(20)]
        public string? CodigoPostal { get; set; }

        [Required]
        public string TipoProveedor { get; set; } // 'Piezas Terminadas', 'Materia Prima', 'Ambos'

        public string? Notas { get; set; }
    }

    public class SupplierDto
    {
        public int SupplierID { get; set; }
        public string Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? RFC { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? CodigoPostal { get; set; }
        public string TipoProveedor { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Notas { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal TotalComprado { get; set; }
    }

    // ===== ÓRDENES DE COMPRA =====

    public class CreatePurchaseOrderDto
    {
        [Required]
        public int SupplierID { get; set; }

        public DateTime? FechaEntregaEsperada { get; set; }

        public string? Notas { get; set; }

        [Required]
        public List<PurchaseOrderItemDto> Items { get; set; }
    }

    public class PurchaseOrderItemDto
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }
    }

    public class PurchaseOrderDto
    {
        public int PurchaseOrderID { get; set; }
        public int SupplierID { get; set; }
        public string ProveedorNombre { get; set; }
        public string NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntregaEsperada { get; set; }
        public DateTime? FechaEntregaReal { get; set; }
        public decimal Total { get; set; }
        public string Estatus { get; set; }
        public string? Notas { get; set; }
        public string? CreadoPorNombre { get; set; }
        public List<PurchaseOrderDetailDto> Detalles { get; set; }
    }

    public class PurchaseOrderDetailDto
    {
        public int PurchaseOrderDetailID { get; set; }
        public int ProductID { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int CantidadRecibida { get; set; }
    }

    public class ReceivePurchaseOrderDto
    {
        [Required]
        public List<ReceiveItemDto> Items { get; set; }
    }

    public class ReceiveItemDto
    {
        [Required]
        public int PurchaseOrderDetailID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadRecibida { get; set; }
    }

    // ===== DEVOLUCIONES =====

    public class CreateReturnDto
    {
        [Required]
        public int OrderID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public List<ReturnItemDto> Items { get; set; }
    }

    public class ReturnItemDto
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        public string? ImagenURL { get; set; }
    }

    public class ReturnDto
    {
        public int ReturnID { get; set; }
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public string UsuarioNombre { get; set; }
        public string UsuarioEmail { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Motivo { get; set; }
        public string? Descripcion { get; set; }
        public string Estatus { get; set; }
        public decimal? MontoDevolucion { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? NotasAdmin { get; set; }
        public List<ReturnDetailDto> Detalles { get; set; }
    }

    public class ReturnDetailDto
    {
        public int ReturnDetailID { get; set; }
        public int ProductID { get; set; }
        public string ProductoNombre { get; set; }
        public string? ProductoImagen { get; set; }
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
        public string? ImagenURL { get; set; }
    }

    public class UpdateReturnStatusDto
    {
        [Required]
        public string Estatus { get; set; } // 'Aprobada', 'Rechazada', 'En Proceso', 'Completada'

        public decimal? MontoDevolucion { get; set; }

        public string? NotasAdmin { get; set; }
    }
}