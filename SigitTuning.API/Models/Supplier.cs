using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        [MaxLength(200)]
        public string? RazonSocial { get; set; }

        [MaxLength(20)]
        public string? RFC { get; set; }

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
        [MaxLength(50)]
        public string TipoProveedor { get; set; } // 'Piezas Terminadas', 'Materia Prima', 'Ambos'

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public string? Notas { get; set; }

        // Relaciones
        public virtual ICollection<PurchaseOrder>? OrdenesCompra { get; set; }
    }

    [Table("PurchaseOrders")]
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int SupplierID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroOrden { get; set; }

        public DateTime FechaOrden { get; set; } = DateTime.Now;

        public DateTime? FechaEntregaEsperada { get; set; }

        public DateTime? FechaEntregaReal { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estatus { get; set; } = "Pendiente";

        public string? Notas { get; set; }

        public int? CreadoPor { get; set; }

        // Relaciones
        [ForeignKey("SupplierID")]
        public virtual Supplier? Proveedor { get; set; }

        [ForeignKey("CreadoPor")]
        public virtual User? UsuarioCreador { get; set; }

        public virtual ICollection<PurchaseOrderDetail>? Detalles { get; set; }
    }

    [Table("PurchaseOrderDetails")]
    public class PurchaseOrderDetail
    {
        [Key]
        public int PurchaseOrderDetailID { get; set; }

        [Required]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        public int CantidadRecibida { get; set; } = 0;

        // Relaciones
        [ForeignKey("PurchaseOrderID")]
        public virtual PurchaseOrder? OrdenCompra { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Producto { get; set; }
    }

    [Table("Returns")]
    public class Return
    {
        [Key]
        public int ReturnID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estatus { get; set; } = "Solicitada";

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MontoDevolucion { get; set; }

        public DateTime? FechaResolucion { get; set; }

        public string? NotasAdmin { get; set; }

        // Relaciones
        [ForeignKey("OrderID")]
        public virtual Order? Pedido { get; set; }

        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }

        public virtual ICollection<ReturnDetail>? Detalles { get; set; }
    }

    [Table("ReturnDetails")]
    public class ReturnDetail
    {
        [Key]
        public int ReturnDetailID { get; set; }

        [Required]
        public int ReturnID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        // Relaciones
        [ForeignKey("ReturnID")]
        public virtual Return? Devolucion { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Producto { get; set; }
    }
}