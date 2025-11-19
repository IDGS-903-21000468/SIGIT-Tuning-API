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
    [Authorize] // Requiere autenticación
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = GetUserId();

                var cartItems = await _context.ShoppingCartItems
                    .Include(c => c.Producto)
                    .Where(c => c.UserID == userId)
                    .Select(c => new CartItemDto
                    {
                        CartItemID = c.CartItemID,
                        ProductID = c.ProductID,
                        ProductoNombre = c.Producto.Nombre,
                        ProductoImagen = c.Producto.ImagenURL,
                        PrecioUnitario = c.Producto.Precio,
                        Cantidad = c.Cantidad,
                        StockDisponible = c.Producto.Stock
                    })
                    .ToListAsync();

                var cart = new CartDto { Items = cartItems };

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Message = "Carrito obtenido exitosamente",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> AddToCart(AddToCartDto request)
        {
            try
            {
                var userId = GetUserId();

                // Verificar que el producto existe y tiene stock
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductID == request.ProductID && p.Activo);

                if (product == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                if (product.Stock < request.Cantidad)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Stock insuficiente. Solo hay {product.Stock} unidades disponibles"
                    });
                }

                // Verificar si el producto ya está en el carrito
                var existingItem = await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(c => c.UserID == userId && c.ProductID == request.ProductID);

                if (existingItem != null)
                {
                    // Actualizar cantidad
                    existingItem.Cantidad += request.Cantidad;

                    // Validar que no exceda el stock
                    if (existingItem.Cantidad > product.Stock)
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = $"No puedes agregar más unidades. Stock disponible: {product.Stock}"
                        });
                    }
                }
                else
                {
                    // Agregar nuevo item al carrito
                    var cartItem = new ShoppingCartItem
                    {
                        UserID = userId,
                        ProductID = request.ProductID,
                        Cantidad = request.Cantidad
                    };
                    _context.ShoppingCartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Producto agregado al carrito"
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

        // PUT: api/Cart/5
        [HttpPut("{cartItemId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateCartItem(int cartItemId, [FromBody] int cantidad)
        {
            try
            {
                var userId = GetUserId();

                var cartItem = await _context.ShoppingCartItems
                    .Include(c => c.Producto)
                    .FirstOrDefaultAsync(c => c.CartItemID == cartItemId && c.UserID == userId);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Item no encontrado en el carrito"
                    });
                }

                if (cantidad <= 0)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "La cantidad debe ser mayor a 0"
                    });
                }

                if (cantidad > cartItem.Producto.Stock)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Stock insuficiente. Solo hay {cartItem.Producto.Stock} unidades disponibles"
                    });
                }

                cartItem.Cantidad = cantidad;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Cantidad actualizada"
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

        // DELETE: api/Cart/5
        [HttpDelete("{cartItemId}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetUserId();

                var cartItem = await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(c => c.CartItemID == cartItemId && c.UserID == userId);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Item no encontrado en el carrito"
                    });
                }

                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Producto eliminado del carrito"
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

        // DELETE: api/Cart/clear
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<string>>> ClearCart()
        {
            try
            {
                var userId = GetUserId();

                var cartItems = await _context.ShoppingCartItems
                    .Where(c => c.UserID == userId)
                    .ToListAsync();

                _context.ShoppingCartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Carrito vaciado"
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