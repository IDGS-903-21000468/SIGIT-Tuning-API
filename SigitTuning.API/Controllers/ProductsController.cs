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
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
        [Authorize]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromForm] CreateProductDto request)
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
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    Anio = request.Anio,
                    Activo = true
                };

                if (request.Imagen != null && request.Imagen.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(request.Imagen.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new ApiResponse<ProductDto>
                        {
                            Success = false,
                            Message = "Solo se permiten archivos de imagen (jpg, png, gif, webp)"
                        });
                    }

                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "products");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Imagen.CopyToAsync(stream);
                    }

                    product.ImagenURL = $"{Request.Scheme}://{Request.Host}/uploads/products/{uniqueFileName}";
                }
                else
                {
                    product.ImagenURL = request.ImagenURL;
                }

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

        // PUT: api/Products/5 (EDITAR PRODUCTO) ✅ NUEVA FUNCIÓN
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromForm] CreateProductDto request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                // Actualizar campos básicos
                product.CategoryID = request.CategoryID;
                product.Nombre = request.Nombre;
                product.Descripcion = request.Descripcion;
                product.Precio = request.Precio;
                product.Stock = request.Stock;
                product.Marca = request.Marca;
                product.Modelo = request.Modelo;
                product.Anio = request.Anio;

                // Actualizar imagen si se envía una nueva
                if (request.Imagen != null && request.Imagen.Length > 0)
                {
                    // Validar extensión
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(request.Imagen.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new ApiResponse<ProductDto>
                        {
                            Success = false,
                            Message = "Solo se permiten archivos de imagen (jpg, png, gif, webp)"
                        });
                    }

                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(product.ImagenURL))
                    {
                        var oldFileName = Path.GetFileName(product.ImagenURL);
                        var oldFilePath = Path.Combine(_env.WebRootPath, "uploads", "products", oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Guardar nueva imagen
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "products");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Imagen.CopyToAsync(stream);
                    }

                    product.ImagenURL = $"{Request.Scheme}://{Request.Host}/uploads/products/{uniqueFileName}";
                }

                _context.Products.Update(product);
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

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Producto actualizado exitosamente",
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

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Producto no encontrado" });

                if (!string.IsNullOrEmpty(product.ImagenURL))
                {
                    var fileName = Path.GetFileName(product.ImagenURL);
                    var filePath = Path.Combine(_env.WebRootPath, "uploads", "products", fileName);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Producto eliminado correctamente"
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

        // POST: api/Products/categories (ADMIN)
        [HttpPost("categories")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromForm] CreateCategoryDto request)
        {
            try
            {
                var category = new ProductCategory
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Activo = true
                };

                if (request.Imagen != null && request.Imagen.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(request.Imagen.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new ApiResponse<CategoryDto>
                        {
                            Success = false,
                            Message = "Solo se permiten archivos de imagen (jpg, png, gif, webp)"
                        });
                    }

                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "categories");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Imagen.CopyToAsync(stream);
                    }

                    category.ImagenURL = $"{Request.Scheme}://{Request.Host}/uploads/categories/{uniqueFileName}";
                }
                else
                {
                    category.ImagenURL = request.ImagenURL;
                }

                _context.ProductCategories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    CategoryID = category.CategoryID,
                    Nombre = category.Nombre,
                    Descripcion = category.Descripcion,
                    ImagenURL = category.ImagenURL
                };

                return CreatedAtAction(nameof(GetCategories), new { id = category.CategoryID },
                    new ApiResponse<CategoryDto>
                    {
                        Success = true,
                        Message = "Categoría creada exitosamente",
                        Data = categoryDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // DELETE: api/Products/categories/5 (ADMIN)
        [HttpDelete("categories/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.ProductCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Categoría no encontrada"
                    });
                }

                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryID == id);
                if (hasProducts)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No se puede eliminar la categoría porque tiene productos asociados"
                    });
                }

                if (!string.IsNullOrEmpty(category.ImagenURL))
                {
                    var fileName = Path.GetFileName(category.ImagenURL);
                    var filePath = Path.Combine(_env.WebRootPath, "uploads", "categories", fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.ProductCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Categoría eliminada correctamente"
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