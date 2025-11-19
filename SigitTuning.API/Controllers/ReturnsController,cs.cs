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
    public class ReturnsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReturnsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/Returns (mis devoluciones)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ReturnDto>>>> GetMyReturns()
        {
            try
            {
                var userId = GetUserId();

                var returns = await _context.Returns
                    .Include(r => r.Usuario)
                    .Include(r => r.Pedido)
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(r => r.UserID == userId)
                    .OrderByDescending(r => r.FechaSolicitud)
                    .Select(r => new ReturnDto
                    {
                        ReturnID = r.ReturnID,
                        OrderID = r.OrderID,
                        UserID = r.UserID,
                        UsuarioNombre = r.Usuario.Nombre,
                        UsuarioEmail = r.Usuario.Email,
                        FechaSolicitud = r.FechaSolicitud,
                        Motivo = r.Motivo,
                        Descripcion = r.Descripcion,
                        Estatus = r.Estatus,
                        MontoDevolucion = r.MontoDevolucion,
                        FechaResolucion = r.FechaResolucion,
                        NotasAdmin = r.NotasAdmin,
                        Detalles = r.Detalles.Select(d => new ReturnDetailDto
                        {
                            ReturnDetailID = d.ReturnDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            Motivo = d.Motivo,
                            ImagenURL = d.ImagenURL
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ReturnDto>>
                {
                    Success = true,
                    Message = "Devoluciones obtenidas exitosamente",
                    Data = returns
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ReturnDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Returns/all (todas las devoluciones - ADMIN)
        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<ReturnDto>>>> GetAllReturns()
        {
            try
            {
                var returns = await _context.Returns
                    .Include(r => r.Usuario)
                    .Include(r => r.Pedido)
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(r => r.FechaSolicitud)
                    .Select(r => new ReturnDto
                    {
                        ReturnID = r.ReturnID,
                        OrderID = r.OrderID,
                        UserID = r.UserID,
                        UsuarioNombre = r.Usuario.Nombre,
                        UsuarioEmail = r.Usuario.Email,
                        FechaSolicitud = r.FechaSolicitud,
                        Motivo = r.Motivo,
                        Descripcion = r.Descripcion,
                        Estatus = r.Estatus,
                        MontoDevolucion = r.MontoDevolucion,
                        FechaResolucion = r.FechaResolucion,
                        NotasAdmin = r.NotasAdmin,
                        Detalles = r.Detalles.Select(d => new ReturnDetailDto
                        {
                            ReturnDetailID = d.ReturnDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            Motivo = d.Motivo,
                            ImagenURL = d.ImagenURL
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ReturnDto>>
                {
                    Success = true,
                    Message = "Todas las devoluciones obtenidas exitosamente",
                    Data = returns
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ReturnDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Returns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReturnDto>>> GetReturn(int id)
        {
            try
            {
                var userId = GetUserId();

                var returnDto = await _context.Returns
                    .Include(r => r.Usuario)
                    .Include(r => r.Pedido)
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(r => r.ReturnID == id && r.UserID == userId)
                    .Select(r => new ReturnDto
                    {
                        ReturnID = r.ReturnID,
                        OrderID = r.OrderID,
                        UserID = r.UserID,
                        UsuarioNombre = r.Usuario.Nombre,
                        UsuarioEmail = r.Usuario.Email,
                        FechaSolicitud = r.FechaSolicitud,
                        Motivo = r.Motivo,
                        Descripcion = r.Descripcion,
                        Estatus = r.Estatus,
                        MontoDevolucion = r.MontoDevolucion,
                        FechaResolucion = r.FechaResolucion,
                        NotasAdmin = r.NotasAdmin,
                        Detalles = r.Detalles.Select(d => new ReturnDetailDto
                        {
                            ReturnDetailID = d.ReturnDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            Motivo = d.Motivo,
                            ImagenURL = d.ImagenURL
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (returnDto == null)
                {
                    return NotFound(new ApiResponse<ReturnDto>
                    {
                        Success = false,
                        Message = "Devolución no encontrada"
                    });
                }

                return Ok(new ApiResponse<ReturnDto>
                {
                    Success = true,
                    Message = "Devolución encontrada",
                    Data = returnDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ReturnDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Returns
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReturnDto>>> CreateReturn(CreateReturnDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();

                // Verificar que el pedido existe y pertenece al usuario
                var order = await _context.Orders
                    .Include(o => o.Detalles)
                    .FirstOrDefaultAsync(o => o.OrderID == request.OrderID && o.UserID == userId);

                if (order == null)
                {
                    return NotFound(new ApiResponse<ReturnDto>
                    {
                        Success = false,
                        Message = "Pedido no encontrado"
                    });
                }

                // Verificar que no haya una devolución pendiente para este pedido
                var existingReturn = await _context.Returns
                    .AnyAsync(r => r.OrderID == request.OrderID &&
                        (r.Estatus == "Solicitada" || r.Estatus == "Aprobada" || r.Estatus == "En Proceso"));

                if (existingReturn)
                {
                    return BadRequest(new ApiResponse<ReturnDto>
                    {
                        Success = false,
                        Message = "Ya existe una devolución pendiente para este pedido"
                    });
                }

                // Crear devolución
                var returnEntity = new Return
                {
                    OrderID = request.OrderID,
                    UserID = userId,
                    FechaSolicitud = DateTime.Now,
                    Motivo = request.Motivo,
                    Descripcion = request.Descripcion,
                    Estatus = "Solicitada"
                };

                _context.Returns.Add(returnEntity);
                await _context.SaveChangesAsync();

                // Crear detalles de devolución
                foreach (var item in request.Items)
                {
                    var detail = new ReturnDetail
                    {
                        ReturnID = returnEntity.ReturnID,
                        ProductID = item.ProductID,
                        Cantidad = item.Cantidad,
                        Motivo = item.Motivo,
                        ImagenURL = item.ImagenURL
                    };

                    _context.ReturnDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Obtener devolución completa
                var returnDto = await _context.Returns
                    .Include(r => r.Usuario)
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(r => r.ReturnID == returnEntity.ReturnID)
                    .Select(r => new ReturnDto
                    {
                        ReturnID = r.ReturnID,
                        OrderID = r.OrderID,
                        UserID = r.UserID,
                        UsuarioNombre = r.Usuario.Nombre,
                        UsuarioEmail = r.Usuario.Email,
                        FechaSolicitud = r.FechaSolicitud,
                        Motivo = r.Motivo,
                        Descripcion = r.Descripcion,
                        Estatus = r.Estatus,
                        Detalles = r.Detalles.Select(d => new ReturnDetailDto
                        {
                            ReturnDetailID = d.ReturnDetailID,
                            ProductID = d.ProductID,
                            ProductoNombre = d.Producto.Nombre,
                            ProductoImagen = d.Producto.ImagenURL,
                            Cantidad = d.Cantidad,
                            Motivo = d.Motivo,
                            ImagenURL = d.ImagenURL
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetReturn), new { id = returnEntity.ReturnID },
                    new ApiResponse<ReturnDto>
                    {
                        Success = true,
                        Message = "Solicitud de devolución creada exitosamente",
                        Data = returnDto
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<ReturnDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // PUT: api/Returns/5/status (ADMIN)
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateReturnStatus(int id, UpdateReturnStatusDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var returnEntity = await _context.Returns
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(r => r.ReturnID == id);

                if (returnEntity == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Devolución no encontrada"
                    });
                }

                var estatusValidos = new[] { "Solicitada", "Aprobada", "Rechazada", "En Proceso", "Completada" };
                if (!estatusValidos.Contains(request.Estatus))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Estatus inválido"
                    });
                }

                returnEntity.Estatus = request.Estatus;
                returnEntity.NotasAdmin = request.NotasAdmin;

                if (request.Estatus == "Completada")
                {
                    returnEntity.FechaResolucion = DateTime.Now;
                    returnEntity.MontoDevolucion = request.MontoDevolucion;

                    // Devolver productos al stock
                    foreach (var detail in returnEntity.Detalles)
                    {
                        detail.Producto.Stock += detail.Cantidad;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Estatus de devolución actualizado exitosamente"
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
    }
}