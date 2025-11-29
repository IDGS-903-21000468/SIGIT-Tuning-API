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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // ========================================
        // 🆕 NUEVO ENDPOINT PARA ADMIN - VER TODOS LOS PEDIDOS
        // ========================================
        // GET: api/Orders/admin/all
        [HttpGet("admin/all")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAllOrdersAdmin()
        {
            try
            {
                // Obtener TODOS los pedidos sin filtrar por usuario
                var orders = await _context.Orders
                    .Include(o => o.Usuario)
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(o => o.FechaPedido)
                    .Select(o => new OrderDto
                    {
                        OrderID = o.OrderID,
                        UserID = o.UserID,
                        UsuarioNombre = o.Usuario.Nombre,
                        UsuarioEmail = o.Usuario.Email,
                        FechaPedido = o.FechaPedido,
                        Total = o.Total,
                        Estatus = o.Estatus,
                        DireccionEnvio = o.DireccionEnvio,
                        Ciudad = o.Ciudad,
                        Estado = o.Estado,
                        NumeroSeguimiento = o.NumeroSeguimiento,
                        Detalles = o.Detalles.Select(d => new OrderDetailDto
                        {
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<OrderDto>>
                {
                    Success = true,
                    Message = $"{orders.Count} pedidos encontrados",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Orders (Mis pedidos)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
        {
            try
            {
                var userId = GetUserId();

                var orders = await _context.Orders
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(o => o.UserID == userId)
                    .OrderByDescending(o => o.FechaPedido)
                    .Select(o => new OrderDto
                    {
                        OrderID = o.OrderID,
                        UserID = userId,
                        FechaPedido = o.FechaPedido,
                        Total = o.Total,
                        Estatus = o.Estatus,
                        DireccionEnvio = o.DireccionEnvio,
                        Ciudad = o.Ciudad,
                        Estado = o.Estado,
                        NumeroSeguimiento = o.NumeroSeguimiento,
                        Detalles = o.Detalles.Select(d => new OrderDetailDto
                        {
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<OrderDto>>
                {
                    Success = true,
                    Message = "Pedidos obtenidos exitosamente",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
        {
            try
            {
                var userId = GetUserId();

                var order = await _context.Orders
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(o => o.OrderID == id && o.UserID == userId)
                    .Select(o => new OrderDto
                    {
                        OrderID = o.OrderID,
                        UserID = o.UserID,
                        FechaPedido = o.FechaPedido,
                        Total = o.Total,
                        Estatus = o.Estatus,
                        DireccionEnvio = o.DireccionEnvio,
                        Ciudad = o.Ciudad,
                        Estado = o.Estado,
                        NumeroSeguimiento = o.NumeroSeguimiento,
                        Detalles = o.Detalles.Select(d => new OrderDetailDto
                        {
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "Pedido no encontrado"
                    });
                }

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Pedido encontrado",
                    Data = order
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Orders (Crear pedido desde el carrito)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder(CreateOrderDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();

                // Obtener items del carrito
                var cartItems = await _context.ShoppingCartItems
                    .Include(c => c.Producto)
                    .Where(c => c.UserID == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return BadRequest(new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "El carrito está vacío"
                    });
                }

                // Validar stock para todos los productos
                foreach (var item in cartItems)
                {
                    if (item.Producto.Stock < item.Cantidad)
                    {
                        return BadRequest(new ApiResponse<OrderDto>
                        {
                            Success = false,
                            Message = $"Stock insuficiente para {item.Producto.Nombre}. Disponible: {item.Producto.Stock}"
                        });
                    }
                }

                // Calcular total
                decimal total = cartItems.Sum(c => c.Producto.Precio * c.Cantidad);

                // Crear la orden
                var order = new Order
                {
                    UserID = userId,
                    FechaPedido = DateTime.Now,
                    Total = total,
                    Estatus = "Pendiente",
                    DireccionEnvio = request.DireccionEnvio,
                    Ciudad = request.Ciudad,
                    Estado = request.Estado,
                    CodigoPostal = request.CodigoPostal,
                    TelefonoContacto = request.TelefonoContacto,
                    MetodoPago = request.MetodoPago
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Crear detalles de la orden y descontar stock
                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Producto.Precio,
                        Subtotal = item.Producto.Precio * item.Cantidad
                    };

                    _context.OrderDetails.Add(orderDetail);

                    // Descontar stock
                    item.Producto.Stock -= item.Cantidad;
                }

                // Vaciar el carrito
                _context.ShoppingCartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Obtener la orden completa con detalles
                var orderDto = await _context.Orders
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(o => o.OrderID == order.OrderID)
                    .Select(o => new OrderDto
                    {
                        OrderID = o.OrderID,
                        UserID = o.UserID,
                        FechaPedido = o.FechaPedido,
                        Total = o.Total,
                        Estatus = o.Estatus,
                        DireccionEnvio = o.DireccionEnvio,
                        Ciudad = o.Ciudad,
                        Estado = o.Estado,
                        NumeroSeguimiento = o.NumeroSeguimiento,
                        Detalles = o.Detalles.Select(d => new OrderDetailDto
                        {
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID },
                    new ApiResponse<OrderDto>
                    {
                        Success = true,
                        Message = "Pedido creado exitosamente",
                        Data = orderDto
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = $"Error al crear el pedido: {ex.Message}"
                });
            }
        }

        // PUT: api/Orders/5/status (ADMIN - Actualizar estatus)
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateOrderStatus(int id, UpdateOrderStatusDto request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Pedido no encontrado"
                    });
                }

                // Validar estatus válidos
                var estatusValidos = new[] { "Pendiente", "Empacando", "Enviado", "En Tránsito", "Entregado", "Cancelado" };
                if (!estatusValidos.Contains(request.Estatus))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Estatus inválido"
                    });
                }

                order.Estatus = request.Estatus;

                if (!string.IsNullOrEmpty(request.NumeroSeguimiento))
                {
                    order.NumeroSeguimiento = request.NumeroSeguimiento;
                }

                if (request.Estatus == "Enviado" && order.FechaEnvio == null)
                {
                    order.FechaEnvio = DateTime.Now;
                }

                if (request.Estatus == "Entregado" && order.FechaEntrega == null)
                {
                    order.FechaEntrega = DateTime.Now;
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