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
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products/categories
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.ProductCategories
                    .Where(c => c.Activo)
                    .Select(c => new CategoryDto
                    {
                        CategoryID = c.CategoryID,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        ImagenURL = c.ImagenURL
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CategoryDto>>
                {
                    Success = true,
                    Message = "Categorías obtenidas exitosamente",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts([FromQuery] int? categoryId = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo);

                // Filtrar por categoría si se especifica
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryID == categoryId.Value);
                }

                var products = await query
                    .Select(p => new ProductDto
                    {
                        ProductID = p.ProductID,
                        CategoryID = p.CategoryID,
                        CategoriaNombre = p.Categoria.Nombre,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        ImagenURL = p.ImagenURL,
                        Marca = p.Marca,
                        Modelo = p.Modelo,
                        Anio = p.Anio
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ProductDto>>
                {
                    Success = true,
                    Message = "Productos obtenidos exitosamente",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ProductDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Categoria)
                    .Where(p => p.ProductID == id && p.Activo)
                    .Select(p => new ProductDto
                    {
                        ProductID = p.ProductID,
                        CategoryID = p.CategoryID,
                        CategoriaNombre = p.Categoria.Nombre,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        ImagenURL = p.ImagenURL,
                        Marca = p.Marca,
                        Modelo = p.Modelo,
                        Anio = p.Anio
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Producto encontrado",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Products/search?query=turbo
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> SearchProducts([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new ApiResponse<List<ProductDto>>
                    {
                        Success = false,
                        Message = "El parámetro de búsqueda es requerido"
                    });
                }

                var products = await _context.Products
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo &&
                        (p.Nombre.Contains(query) ||
                         p.Descripcion.Contains(query) ||
                         p.Marca.Contains(query) ||
                         p.Modelo.Contains(query)))
                    .Select(p => new ProductDto
                    {
                        ProductID = p.ProductID,
                        CategoryID = p.CategoryID,
                        CategoriaNombre = p.Categoria.Nombre,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        ImagenURL = p.ImagenURL,
                        Marca = p.Marca,
                        Modelo = p.Modelo,
                        Anio = p.Anio
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ProductDto>>
                {
                    Success = true,
                    Message = $"{products.Count} productos encontrados",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ProductDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Products (ADMIN)
        [HttpPost]
        [Authorize] // Requiere autenticación
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(CreateProductDto request)
        {
            try
            {
                var product = new Product
                {
                    CategoryID = request.CategoryID,
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Precio = request.Precio,
                    Stock = request.Stock,
                    ImagenURL = request.ImagenURL,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    Anio = request.Anio,
                    Activo = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var productDto = new ProductDto
                {
                    ProductID = product.ProductID,
                    CategoryID = product.CategoryID,
                    Nombre = product.Nombre,
                    Descripcion = product.Descripcion,
                    Precio = product.Precio,
                    Stock = product.Stock,
                    ImagenURL = product.ImagenURL,
                    Marca = product.Marca,
                    Modelo = product.Modelo,
                    Anio = product.Anio
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID },
                    new ApiResponse<ProductDto>
                    {
                        Success = true,
                        Message = "Producto creado exitosamente",
                        Data = productDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}