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
    public class MarketplaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/Marketplace/listings
        [HttpGet("listings")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<MarketplaceListingDto>>>> GetListings()
        {
            try
            {
                var listings = await _context.MarketplaceListings
                    .Include(l => l.Vendedor)
                    .Include(l => l.Ofertas)
                    .Where(l => l.Estatus == "Activa")
                    .OrderByDescending(l => l.FechaPublicacion)
                    .Select(l => new MarketplaceListingDto
                    {
                        ListingID = l.ListingID,
                        VendedorID = l.UserID_Vendedor,
                        VendedorNombre = l.Vendedor.Nombre,
                        VendedorAvatar = l.Vendedor.AvatarURL,
                        Titulo = l.Titulo,
                        Descripcion = l.Descripcion,
                        ImagenURL = l.ImagenURL,
                        PrecioInicial = l.PrecioInicial,
                        PrecioActual = l.PrecioActual,
                        Marca = l.Marca,
                        Modelo = l.Modelo,
                        Anio = l.Anio,
                        Kilometraje = l.Kilometraje,
                        Modificaciones = l.Modificaciones,
                        FechaPublicacion = l.FechaPublicacion,
                        Estatus = l.Estatus,
                        TotalOfertas = l.Ofertas.Count,
                        MejorOferta = l.Ofertas.Any() ? l.Ofertas.Max(o => o.MontoOferta) : null
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<MarketplaceListingDto>>
                {
                    Success = true,
                    Message = "Listados obtenidos exitosamente",
                    Data = listings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<MarketplaceListingDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Marketplace/listings
        [HttpPost("listings")]
        public async Task<ActionResult<ApiResponse<MarketplaceListingDto>>> CreateListing(CreateMarketplaceListingDto request)
        {
            try
            {
                var userId = GetUserId();

                var listing = new MarketplaceListing
                {
                    UserID_Vendedor = userId,
                    Titulo = request.Titulo,
                    Descripcion = request.Descripcion,
                    ImagenURL = request.ImagenURL,
                    PrecioInicial = request.PrecioInicial,
                    PrecioActual = request.PrecioInicial,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    Anio = request.Anio,
                    Kilometraje = request.Kilometraje,
                    Modificaciones = request.Modificaciones,
                    FechaPublicacion = DateTime.Now,
                    Estatus = "Activa",
                    ComisionPlataforma = 15.00m
                };

                _context.MarketplaceListings.Add(listing);
                await _context.SaveChangesAsync();

                var listingDto = await _context.MarketplaceListings
                    .Include(l => l.Vendedor)
                    .Where(l => l.ListingID == listing.ListingID)
                    .Select(l => new MarketplaceListingDto
                    {
                        ListingID = l.ListingID,
                        VendedorID = l.UserID_Vendedor,
                        VendedorNombre = l.Vendedor.Nombre,
                        VendedorAvatar = l.Vendedor.AvatarURL,
                        Titulo = l.Titulo,
                        Descripcion = l.Descripcion,
                        ImagenURL = l.ImagenURL,
                        PrecioInicial = l.PrecioInicial,
                        PrecioActual = l.PrecioActual,
                        Marca = l.Marca,
                        Modelo = l.Modelo,
                        Anio = l.Anio,
                        Kilometraje = l.Kilometraje,
                        Modificaciones = l.Modificaciones,
                        FechaPublicacion = l.FechaPublicacion,
                        Estatus = l.Estatus,
                        TotalOfertas = 0,
                        MejorOferta = null
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<MarketplaceListingDto>
                {
                    Success = true,
                    Message = "Listado creado exitosamente. Se aplicará una comisión del 15% al vender.",
                    Data = listingDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<MarketplaceListingDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Marketplace/listings/5/bids
        [HttpPost("listings/{listingId}/bids")]
        public async Task<ActionResult<ApiResponse<BidDto>>> CreateBid(int listingId, CreateBidDto request)
        {
            try
            {
                var userId = GetUserId();

                var listing = await _context.MarketplaceListings.FindAsync(listingId);
                if (listing == null || listing.Estatus != "Activa")
                {
                    return BadRequest(new ApiResponse<BidDto>
                    {
                        Success = false,
                        Message = "El listado no existe o no está activo"
                    });
                }

                if (listing.UserID_Vendedor == userId)
                {
                    return BadRequest(new ApiResponse<BidDto>
                    {
                        Success = false,
                        Message = "No puedes ofertar en tu propia publicación"
                    });
                }

                var bid = new MarketplaceBid
                {
                    ListingID = listingId,
                    UserID_Comprador = userId,
                    MontoOferta = request.MontoOferta,
                    Mensaje = request.Mensaje,
                    FechaOferta = DateTime.Now
                };

                _context.MarketplaceBids.Add(bid);

                // Actualizar precio actual si la oferta es mayor
                if (request.MontoOferta > listing.PrecioActual)
                {
                    listing.PrecioActual = request.MontoOferta;
                }

                await _context.SaveChangesAsync();

                var bidDto = await _context.MarketplaceBids
                    .Include(b => b.Comprador)
                    .Where(b => b.BidID == bid.BidID)
                    .Select(b => new BidDto
                    {
                        BidID = b.BidID,
                        CompradorID = b.UserID_Comprador,
                        CompradorNombre = b.Comprador.Nombre,
                        CompradorAvatar = b.Comprador.AvatarURL,
                        MontoOferta = b.MontoOferta,
                        Mensaje = b.Mensaje,
                        FechaOferta = b.FechaOferta,
                        Aceptada = b.Aceptada
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<BidDto>
                {
                    Success = true,
                    Message = "Oferta enviada exitosamente",
                    Data = bidDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<BidDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Marketplace/listings/5/chat (Iniciar chat)
        [HttpPost("listings/{listingId}/chat")]
        public async Task<ActionResult<ApiResponse<int>>> InitiateChat(int listingId)
        {
            try
            {
                var userId = GetUserId();

                var listing = await _context.MarketplaceListings.FindAsync(listingId);
                if (listing == null)
                {
                    return NotFound(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Listado no encontrado"
                    });
                }

                // Verificar si ya existe un chat
                var existingChat = await _context.MarketplaceChats
                    .FirstOrDefaultAsync(c => c.ListingID == listingId &&
                        c.UserID_Comprador == userId);

                if (existingChat != null)
                {
                    return Ok(new ApiResponse<int>
                    {
                        Success = true,
                        Message = "Chat existente",
                        Data = existingChat.ChatID
                    });
                }

                // Crear nuevo chat
                var chat = new MarketplaceChat
                {
                    ListingID = listingId,
                    UserID_Vendedor = listing.UserID_Vendedor,
                    UserID_Comprador = userId,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.MarketplaceChats.Add(chat);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = "Chat iniciado",
                    Data = chat.ChatID
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Marketplace/chats/5/messages
        [HttpGet("chats/{chatId}/messages")]
        public async Task<ActionResult<ApiResponse<ChatDto>>> GetChatMessages(int chatId)
        {
            try
            {
                var userId = GetUserId();

                var chat = await _context.MarketplaceChats
                    .Include(c => c.Publicacion)
                    .Include(c => c.Vendedor)
                    .Include(c => c.Comprador)
                    .Include(c => c.Mensajes)
                        .ThenInclude(m => m.Remitente)
                    .FirstOrDefaultAsync(c => c.ChatID == chatId &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId));

                if (chat == null)
                {
                    return NotFound(new ApiResponse<ChatDto>
                    {
                        Success = false,
                        Message = "Chat no encontrado"
                    });
                }

                var otroUsuario = chat.UserID_Vendedor == userId ? chat.Comprador : chat.Vendedor;

                var chatDto = new ChatDto
                {
                    ChatID = chat.ChatID,
                    ListingID = chat.ListingID,
                    ListingTitulo = chat.Publicacion.Titulo,
                    OtroUsuarioID = otroUsuario.UserID,
                    OtroUsuarioNombre = otroUsuario.Nombre,
                    OtroUsuarioAvatar = otroUsuario.AvatarURL,
                    Mensajes = chat.Mensajes.Select(m => new ChatMessageDto
                    {
                        MessageID = m.MessageID,
                        SenderUserID = m.SenderUserID,
                        SenderNombre = m.Remitente.Nombre,
                        Mensaje = m.Mensaje,
                        FechaEnvio = m.FechaEnvio,
                        EsPropio = m.SenderUserID == userId
                    }).OrderBy(m => m.FechaEnvio).ToList()
                };

                return Ok(new ApiResponse<ChatDto>
                {
                    Success = true,
                    Message = "Mensajes obtenidos",
                    Data = chatDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ChatDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Marketplace/chats/5/messages
        [HttpPost("chats/{chatId}/messages")]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(int chatId, CreateChatMessageDto request)
        {
            try
            {
                var userId = GetUserId();

                var chat = await _context.MarketplaceChats
                    .FirstOrDefaultAsync(c => c.ChatID == chatId &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId));

                if (chat == null)
                {
                    return NotFound(new ApiResponse<ChatMessageDto>
                    {
                        Success = false,
                        Message = "Chat no encontrado"
                    });
                }

                var message = new ChatMessage
                {
                    ChatID = chatId,
                    SenderUserID = userId,
                    Mensaje = request.Mensaje,
                    FechaEnvio = DateTime.Now,
                    Leido = false
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                var messageDto = await _context.ChatMessages
                    .Include(m => m.Remitente)
                    .Where(m => m.MessageID == message.MessageID)
                    .Select(m => new ChatMessageDto
                    {
                        MessageID = m.MessageID,
                        SenderUserID = m.SenderUserID,
                        SenderNombre = m.Remitente.Nombre,
                        Mensaje = m.Mensaje,
                        FechaEnvio = m.FechaEnvio,
                        EsPropio = true
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<ChatMessageDto>
                {
                    Success = true,
                    Message = "Mensaje enviado",
                    Data = messageDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ChatMessageDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}