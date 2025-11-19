using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using System.Security.Claims;

namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Solo usuarios autenticados (admin)
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/PurchaseOrders
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PurchaseOrderDto>>>> GetPurchaseOrders()
        {
            try
            {
                var orders = await _context.PurchaseOrders
                    .Include(po => po.Proveedor)
                    .Include(po => po.UsuarioCreador)
                    .Include(po => po.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(po => po.FechaOrden)
                    .Select(po => new PurchaseOrderDto
                    {
                        PurchaseOrderID = po.PurchaseOrderID,
                        SupplierID = po.SupplierID,
                        ProveedorNombre = po.Proveedor.Nombre,
                        NumeroOrden = po.NumeroOrden,
                        FechaOrden = po.FechaOrden,
                        FechaEntregaEsperada = po.FechaEntregaEsperada,
                        FechaEntregaReal = po.FechaEntregaReal,
                        Total = po.Total,
                        Estatus = po.Estatus,
                        Notas = po.Notas,
                        CreadoPorNombre = po.UsuarioCreador.Nombre,
                        Detalles = po.Detalles.Select(d => new PurchaseOrderDetailDto
                        {
                            PurchaseOrderDetailID = d.PurchaseOrderDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal,
                            CantidadRecibida = d.CantidadRecibida
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<PurchaseOrderDto>>
                {
                    Success = true,
                    Message = "Órdenes de compra obtenidas exitosamente",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<PurchaseOrderDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/PurchaseOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetPurchaseOrder(int id)
        {
            try
            {
                var order = await _context.PurchaseOrders
                    .Include(po => po.Proveedor)
                    .Include(po => po.UsuarioCreador)
                    .Include(po => po.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(po => po.PurchaseOrderID == id)
                    .Select(po => new PurchaseOrderDto
                    {
                        PurchaseOrderID = po.PurchaseOrderID,
                        SupplierID = po.SupplierID,
                        ProveedorNombre = po.Proveedor.Nombre,
                        NumeroOrden = po.NumeroOrden,
                        FechaOrden = po.FechaOrden,
                        FechaEntregaEsperada = po.FechaEntregaEsperada,
                        FechaEntregaReal = po.FechaEntregaReal,
                        Total = po.Total,
                        Estatus = po.Estatus,
                        Notas = po.Notas,
                        CreadoPorNombre = po.UsuarioCreador.Nombre,
                        Detalles = po.Detalles.Select(d => new PurchaseOrderDetailDto
                        {
                            PurchaseOrderDetailID = d.PurchaseOrderDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal,
                            CantidadRecibida = d.CantidadRecibida
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new ApiResponse<PurchaseOrderDto>
                    {
                        Success = false,
                        Message = "Orden de compra no encontrada"
                    });
                }

                return Ok(new ApiResponse<PurchaseOrderDto>
                {
                    Success = true,
                    Message = "Orden de compra encontrada",
                    Data = order
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PurchaseOrderDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/PurchaseOrders
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreatePurchaseOrder(CreatePurchaseOrderDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();

                // Generar número de orden único
                var numeroOrden = $"PO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                // Calcular total
                decimal total = request.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

                // Crear orden de compra
                var purchaseOrder = new PurchaseOrder
                {
                    SupplierID = request.SupplierID,
                    NumeroOrden = numeroOrden,
                    FechaOrden = DateTime.Now,
                    FechaEntregaEsperada = request.FechaEntregaEsperada,
                    Total = total,
                    Estatus = "Pendiente",
                    Notas = request.Notas,
                    CreadoPor = userId
                };

                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // Crear detalles
                foreach (var item in request.Items)
                {
                    var detail = new PurchaseOrderDetail
                    {
                        PurchaseOrderID = purchaseOrder.PurchaseOrderID,
                        ProductID = item.ProductID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Cantidad * item.PrecioUnitario,
                        CantidadRecibida = 0
                    };

                    _context.PurchaseOrderDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Obtener orden completa
                var orderDto = await _context.PurchaseOrders
                    .Include(po => po.Proveedor)
                    .Include(po => po.UsuarioCreador)
                    .Include(po => po.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(po => po.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
                    .Select(po => new PurchaseOrderDto
                    {
                        PurchaseOrderID = po.PurchaseOrderID,
                        SupplierID = po.SupplierID,
                        ProveedorNombre = po.Proveedor.Nombre,
                        NumeroOrden = po.NumeroOrden,
                        FechaOrden = po.FechaOrden,
                        FechaEntregaEsperada = po.FechaEntregaEsperada,
                        Total = po.Total,
                        Estatus = po.Estatus,
                        Notas = po.Notas,
                        CreadoPorNombre = po.UsuarioCreador.Nombre,
                        Detalles = po.Detalles.Select(d => new PurchaseOrderDetailDto
                        {
                            PurchaseOrderDetailID = d.PurchaseOrderDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal,
                            CantidadRecibida = d.CantidadRecibida
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.PurchaseOrderID },
                    new ApiResponse<PurchaseOrderDto>
                    {
                        Success = true,
                        Message = "Orden de compra creada exitosamente",
                        Data = orderDto
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<PurchaseOrderDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // PUT: api/PurchaseOrders/5/receive
        [HttpPut("{id}/receive")]
        public async Task<ActionResult<ApiResponse<string>>> ReceivePurchaseOrder(int id, ReceivePurchaseOrderDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(po => po.PurchaseOrderID == id);

                if (purchaseOrder == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Orden de compra no encontrada"
                    });
                }

                // Actualizar cantidades recibidas y stock
                foreach (var item in request.Items)
                {
                    var detail = purchaseOrder.Detalles.FirstOrDefault(d => d.PurchaseOrderDetailID == item.PurchaseOrderDetailID);
                    if (detail != null)
                    {
                        detail.CantidadRecibida += item.CantidadRecibida;

                        // Actualizar stock del producto
                        detail.Producto.Stock += item.CantidadRecibida;
                    }
                }

                // Verificar si se recibió todo
                bool todoRecibido = purchaseOrder.Detalles.All(d => d.CantidadRecibida >= d.Cantidad);

                if (todoRecibido)
                {
                    purchaseOrder.Estatus = "Recibida";
                    purchaseOrder.FechaEntregaReal = DateTime.Now;
                }
                else
                {
                    purchaseOrder.Estatus = "Parcialmente Recibida";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Productos recibidos y stock actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // PUT: api/PurchaseOrders/5/status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromBody] string estatus)
        {
            try
            {
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);

                if (purchaseOrder == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Orden de compra no encontrada"
                    });
                }

                var estatusValidos = new[] { "Pendiente", "Confirmada", "En Tránsito", "Recibida", "Cancelada" };
                if (!estatusValidos.Contains(estatus))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Estatus inválido"
                    });
                }

                purchaseOrder.Estatus = estatus;

                if (estatus == "Recibida")
                {
                    purchaseOrder.FechaEntregaReal = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Estatus actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}