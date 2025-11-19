using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;

namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Solo usuarios autenticados (admin)
    public class SuppliersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Suppliers
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetSuppliers()
        {
            try
            {
                var suppliers = await _context.Suppliers
                    .Where(s => s.Activo)
                    .Select(s => new SupplierDto
                    {
                        SupplierID = s.SupplierID,
                        Nombre = s.Nombre,
                        RazonSocial = s.RazonSocial,
                        RFC = s.RFC,
                        Email = s.Email,
                        Telefono = s.Telefono,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Estado = s.Estado,
                        CodigoPostal = s.CodigoPostal,
                        TipoProveedor = s.TipoProveedor,
                        Activo = s.Activo,
                        FechaRegistro = s.FechaRegistro,
                        Notas = s.Notas,
                        TotalOrdenes = s.OrdenesCompra.Count,
                        TotalComprado = s.OrdenesCompra.Sum(o => o.Total)
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<SupplierDto>>
                {
                    Success = true,
                    Message = "Proveedores obtenidos exitosamente",
                    Data = suppliers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<SupplierDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Where(s => s.SupplierID == id)
                    .Select(s => new SupplierDto
                    {
                        SupplierID = s.SupplierID,
                        Nombre = s.Nombre,
                        RazonSocial = s.RazonSocial,
                        RFC = s.RFC,
                        Email = s.Email,
                        Telefono = s.Telefono,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Estado = s.Estado,
                        CodigoPostal = s.CodigoPostal,
                        TipoProveedor = s.TipoProveedor,
                        Activo = s.Activo,
                        FechaRegistro = s.FechaRegistro,
                        Notas = s.Notas,
                        TotalOrdenes = s.OrdenesCompra.Count,
                        TotalComprado = s.OrdenesCompra.Sum(o => o.Total)
                    })
                    .FirstOrDefaultAsync();

                if (supplier == null)
                {
                    return NotFound(new ApiResponse<SupplierDto>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado"
                    });
                }

                return Ok(new ApiResponse<SupplierDto>
                {
                    Success = true,
                    Message = "Proveedor encontrado",
                    Data = supplier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Suppliers
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier(CreateSupplierDto request)
        {
            try
            {
                var supplier = new Supplier
                {
                    Nombre = request.Nombre,
                    RazonSocial = request.RazonSocial,
                    RFC = request.RFC,
                    Email = request.Email,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion,
                    Ciudad = request.Ciudad,
                    Estado = request.Estado,
                    CodigoPostal = request.CodigoPostal,
                    TipoProveedor = request.TipoProveedor,
                    Notas = request.Notas,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                var supplierDto = new SupplierDto
                {
                    SupplierID = supplier.SupplierID,
                    Nombre = supplier.Nombre,
                    RazonSocial = supplier.RazonSocial,
                    RFC = supplier.RFC,
                    Email = supplier.Email,
                    Telefono = supplier.Telefono,
                    Direccion = supplier.Direccion,
                    Ciudad = supplier.Ciudad,
                    Estado = supplier.Estado,
                    CodigoPostal = supplier.CodigoPostal,
                    TipoProveedor = supplier.TipoProveedor,
                    Activo = supplier.Activo,
                    FechaRegistro = supplier.FechaRegistro,
                    Notas = supplier.Notas,
                    TotalOrdenes = 0,
                    TotalComprado = 0
                };

                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierID },
                    new ApiResponse<SupplierDto>
                    {
                        Success = true,
                        Message = "Proveedor creado exitosamente",
                        Data = supplierDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // PUT: api/Suppliers/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateSupplier(int id, CreateSupplierDto request)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);

                if (supplier == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado"
                    });
                }

                supplier.Nombre = request.Nombre;
                supplier.RazonSocial = request.RazonSocial;
                supplier.RFC = request.RFC;
                supplier.Email = request.Email;
                supplier.Telefono = request.Telefono;
                supplier.Direccion = request.Direccion;
                supplier.Ciudad = request.Ciudad;
                supplier.Estado = request.Estado;
                supplier.CodigoPostal = request.CodigoPostal;
                supplier.TipoProveedor = request.TipoProveedor;
                supplier.Notas = request.Notas;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Proveedor actualizado exitosamente"
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

        // DELETE: api/Suppliers/5 (desactivar)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);

                if (supplier == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado"
                    });
                }

                supplier.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Proveedor desactivado exitosamente"
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