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

        // ================================================================
        // GET: api/Marketplace/listings?search=civic&includePending=false
        // ================================================================
        [HttpGet("listings")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<MarketplaceListingDto>>>> GetListings(
            [FromQuery] string? search = null,
            [FromQuery] bool includePending = false)
        {
            try
            {
                var query = _context.MarketplaceListings
                    .Include(l => l.Vendedor)
                    .Include(l => l.Ofertas)
                    .AsQueryable();

                // Filtrar por estatus
                if (!includePending)
                {
                    query = query.Where(l => l.Estatus == "Activa");
                }

                // 🔍 FILTRO DE BÚSQUEDA
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(l =>
                        l.Titulo.ToLower().Contains(searchLower) ||
                        (l.Marca != null && l.Marca.ToLower().Contains(searchLower)) ||
                        (l.Modelo != null && l.Modelo.ToLower().Contains(searchLower)) ||
                        (l.Descripcion != null && l.Descripcion.ToLower().Contains(searchLower))
                    );
                }

                var listings = await query
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
                    Estatus = "Pendiente",
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
                        FechaPublicacion = l.FechaPublicacion,
                        Estatus = l.Estatus,
                        TotalOfertas = 0
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<MarketplaceListingDto>
                {
                    Success = true,
                    Message = "Listado creado exitosamente",
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

        // POST: Bids
        [HttpPost("listings/{listingId}/bids")]
        public async Task<ActionResult<ApiResponse<BidDto>>> CreateBid(int listingId, CreateBidDto request)
        {
            try
            {
                var userId = GetUserId();
                var listing = await _context.MarketplaceListings.FindAsync(listingId);

                if (listing == null || listing.Estatus != "Activa")
                    return BadRequest(new ApiResponse<BidDto>
                    {
                        Success = false,
                        Message = "Listado no disponible"
                    });

                if (listing.UserID_Vendedor == userId)
                    return BadRequest(new ApiResponse<BidDto>
                    {
                        Success = false,
                        Message = "No puedes ofertar en tu propia publicación"
                    });

                var bid = new MarketplaceBid
                {
                    ListingID = listingId,
                    UserID_Comprador = userId,
                    MontoOferta = request.MontoOferta,
                    Mensaje = request.Mensaje,
                    FechaOferta = DateTime.Now
                };

                _context.MarketplaceBids.Add(bid);

                if (request.MontoOferta > listing.PrecioActual)
                    listing.PrecioActual = request.MontoOferta;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<BidDto>
                {
                    Success = true,
                    Message = "Oferta enviada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<BidDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ================================================================
        // POST: api/Marketplace/listings/{listingId}/chat
        // INICIAR O RECUPERAR CHAT
        // ================================================================
        [HttpPost("listings/{listingId}/chat")]
        public async Task<ActionResult<ApiResponse<int>>> InitiateChat(int listingId)
        {
            try
            {
                var userId = GetUserId();
                var listing = await _context.MarketplaceListings.FindAsync(listingId);

                if (listing == null)
                    return NotFound(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Listado no encontrado"
                    });

                // ✅ PRIMERO: Buscar si ya existe un chat (como vendedor O como comprador)
                var existingChat = await _context.MarketplaceChats
                    .FirstOrDefaultAsync(c =>
                        c.ListingID == listingId &&
                        c.Activo &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId));

                if (existingChat != null)
                {
                    return Ok(new ApiResponse<int>
                    {
                        Success = true,
                        Message = "Chat existente",
                        Data = existingChat.ChatID
                    });
                }

                // ✅ SEGUNDO: Solo validar si intenta CREAR un nuevo chat siendo vendedor
                if (listing.UserID_Vendedor == userId)
                {
                    return BadRequest(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "No puedes iniciar un chat con tu propia publicación. Espera a que un comprador te contacte."
                    });
                }

                // ✅ TERCERO: Crear nuevo chat (solo si es comprador)
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
                    Message = ex.Message
                });
            }
        }

        // ================================================================
        // GET: api/Marketplace/chats/{chatId}/messages
        // OBTENER MENSAJES DEL CHAT
        // ================================================================
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
                        .AsSplitQuery()
                    .FirstOrDefaultAsync(c =>
                        c.ChatID == chatId &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId));

                if (chat == null)
                    return NotFound(new ApiResponse<ChatDto>
                    {
                        Success = false,
                        Message = "Chat no encontrado"
                    });

                // ⚠️ DETERMINAR QUIÉN ES EL OTRO USUARIO
                var soyVendedor = chat.UserID_Vendedor == userId;
                var otroUsuario = soyVendedor ? chat.Comprador : chat.Vendedor;

                var chatDto = new ChatDto
                {
                    ChatID = chat.ChatID,
                    ListingID = chat.ListingID,
                    ListingTitulo = chat.Publicacion?.Titulo ?? "Sin título",
                    ListingImagen = chat.Publicacion?.ImagenURL,
                    ListingEstatus = chat.Publicacion?.Estatus ?? "Desconocido",
                    OtroUsuarioID = otroUsuario.UserID,
                    OtroUsuarioNombre = otroUsuario.Nombre,
                    OtroUsuarioAvatar = otroUsuario.AvatarURL,
                    SoyVendedor = soyVendedor,
                    Mensajes = chat.Mensajes
                        .OrderBy(m => m.FechaEnvio)
                        .Select(m => new ChatMessageDto
                        {
                            MessageID = m.MessageID,
                            SenderUserID = m.SenderUserID,
                            SenderNombre = m.Remitente?.Nombre ?? "Usuario",
                            SenderAvatar = m.Remitente?.AvatarURL,
                            Mensaje = m.Mensaje,
                            FechaEnvio = m.FechaEnvio.ToString("yyyy-MM-dd HH:mm:ss"),
                            EsPropio = m.SenderUserID == userId
                        })
                        .ToList()
                };

                return Ok(new ApiResponse<ChatDto>
                {
                    Success = true,
                    Data = chatDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ChatDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ================================================================
        // POST: api/Marketplace/chats/{chatId}/messages
        // ENVIAR MENSAJE
        // ================================================================
        [HttpPost("chats/{chatId}/messages")]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(
            int chatId,
            CreateChatMessageDto request)
        {
            try
            {
                var userId = GetUserId();
                var chat = await _context.MarketplaceChats
                    .FirstOrDefaultAsync(c =>
                        c.ChatID == chatId &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId));

                if (chat == null)
                    return NotFound(new ApiResponse<ChatMessageDto>
                    {
                        Success = false,
                        Message = "Chat no encontrado"
                    });

                if (string.IsNullOrWhiteSpace(request.Mensaje))
                    return BadRequest(new ApiResponse<ChatMessageDto>
                    {
                        Success = false,
                        Message = "El mensaje no puede estar vacío"
                    });

                var message = new ChatMessage
                {
                    ChatID = chatId,
                    SenderUserID = userId,
                    Mensaje = request.Mensaje.Trim(),
                    FechaEnvio = DateTime.Now,
                    Leido = false
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                var senderName = await _context.Users
                    .Where(u => u.UserID == userId)
                    .Select(u => u.Nombre)
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<ChatMessageDto>
                {
                    Success = true,
                    Message = "Mensaje enviado",
                    Data = new ChatMessageDto
                    {
                        MessageID = message.MessageID,
                        SenderUserID = userId,
                        SenderNombre = senderName ?? "Usuario",
                        Mensaje = message.Mensaje,
                        FechaEnvio = message.FechaEnvio.ToString("yyyy-MM-dd HH:mm:ss"),
                        EsPropio = true
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ChatMessageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ================================================================
        // GET: api/Marketplace/chats
        // OBTENER TODOS MIS CHATS (INBOX)
        // ================================================================
        [HttpGet("chats")]
        public async Task<ActionResult<ApiResponse<List<ChatSummaryDto>>>> GetMyChats()
        {
            try
            {
                var userId = GetUserId();

                var chats = await _context.MarketplaceChats
                    .Include(c => c.Publicacion)
                    .Include(c => c.Vendedor)
                    .Include(c => c.Comprador)
                    .Include(c => c.Mensajes)
                    .Where(c =>
                        c.Activo &&
                        (c.UserID_Vendedor == userId || c.UserID_Comprador == userId))
                    .ToListAsync();

                var chatSummaries = chats
                    .Select(c =>
                    {
                        var soyVendedor = c.UserID_Vendedor == userId;
                        var otroUsuario = soyVendedor ? c.Comprador : c.Vendedor;
                        var ultimoMensajeObj = c.Mensajes?
                            .OrderByDescending(m => m.FechaEnvio)
                            .FirstOrDefault();

                        return new ChatSummaryDto
                        {
                            ChatID = c.ChatID,
                            ListingID = c.ListingID,
                            ListingTitulo = c.Publicacion?.Titulo ?? "Vehículo no disponible",
                            ListingImagen = c.Publicacion?.ImagenURL,
                            OtroUsuarioID = otroUsuario.UserID,
                            OtroUsuarioNombre = otroUsuario.Nombre,
                            OtroUsuarioAvatar = otroUsuario.AvatarURL,
                            UltimoMensaje = ultimoMensajeObj?.Mensaje,
                            FechaUltimoMensaje = ultimoMensajeObj?.FechaEnvio.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                    })
                    .OrderByDescending(c => c.FechaUltimoMensaje)
                    .ToList();

                return Ok(new ApiResponse<List<ChatSummaryDto>>
                {
                    Success = true,
                    Message = "Chats obtenidos",
                    Data = chatSummaries
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ChatSummaryDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // ================================================================
        // POST: api/Marketplace/listings/{listingId}/complete-sale
        // COMPLETAR VENTA (Vendedor o Admin)
        // ================================================================
        [HttpPost("listings/{listingId}/complete-sale")]
        public async Task<ActionResult<ApiResponse<CompleteSaleResponseDto>>> CompleteSale(
            int listingId,
            CompleteSaleDto request)
        {
            try
            {
                var userId = GetUserId();
                var listing = await _context.MarketplaceListings
                    .Include(l => l.Vendedor)
                    .FirstOrDefaultAsync(l => l.ListingID == listingId);

                if (listing == null)
                    return NotFound(new ApiResponse<CompleteSaleResponseDto>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });

                // ✅ PERMITIR AL VENDEDOR O AL ADMIN
                var isVendedor = listing.UserID_Vendedor == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isVendedor && !isAdmin)
                    return StatusCode(403, new ApiResponse<CompleteSaleResponseDto>
                    {
                        Success = false,
                        Message = "No tienes permisos para completar esta venta"
                    });

                if (listing.Estatus != "Activa")
                    return BadRequest(new ApiResponse<CompleteSaleResponseDto>
                    {
                        Success = false,
                        Message = "Esta publicación ya no está activa"
                    });

                // Verificar que el comprador exista
                var comprador = await _context.Users.FindAsync(request.CompradorID);
                if (comprador == null)
                    return BadRequest(new ApiResponse<CompleteSaleResponseDto>
                    {
                        Success = false,
                        Message = "Comprador no encontrado"
                    });

                // Calcular comisión
                var comisionMonto = listing.PrecioActual * (listing.ComisionPlataforma / 100);
                var precioFinal = listing.PrecioActual - comisionMonto;

                // Actualizar publicación
                listing.Estatus = "Vendida";
                listing.CompradorID = request.CompradorID;
                listing.PrecioFinalVenta = precioFinal;
                listing.FechaVenta = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<CompleteSaleResponseDto>
                {
                    Success = true,
                    Message = "¡Venta completada exitosamente!",
                    Data = new CompleteSaleResponseDto
                    {
                        ListingID = listing.ListingID,
                        PrecioVenta = listing.PrecioActual,
                        Comision = comisionMonto,
                        PrecioFinal = precioFinal,
                        CompradorNombre = comprador.Nombre,
                        FechaVenta = listing.FechaVenta.Value
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CompleteSaleResponseDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // ================================================================
        // POST: api/Marketplace/bids/{bidId}/respond
        // RESPONDER A UNA OFERTA (Aceptar/Rechazar)
        // ================================================================
        public class RespondBidRequest
        {
            public string Accion { get; set; } // "aceptar" o "rechazar"
        }

        [HttpPost("bids/{bidId}/respond")]
        public async Task<IActionResult> RespondToBid(int bidId, [FromBody] RespondBidRequest request)
        {
            var bid = await _context.MarketplaceBids
                .Include(b => b.Publicacion)
                .FirstOrDefaultAsync(b => b.BidID == bidId);

            if (bid == null) return NotFound("Oferta no encontrada");

            if (request.Accion.ToLower() == "aceptar")
            {
                bid.Estatus = "Aceptada";
                bid.Aceptada = true;
                bid.FechaRespuesta = DateTime.Now;

                bid.Publicacion.PrecioActual = bid.MontoOferta;

                // Opcional: Rechazar automáticamente otras ofertas pendientes
                var otrasOfertas = await _context.MarketplaceBids
                    .Where(b => b.ListingID == bid.ListingID && b.BidID != bidId && b.Estatus == "Pendiente")
                    .ToListAsync();

                foreach (var otra in otrasOfertas)
                {
                    otra.Estatus = "Rechazada";
                }
            }
            else
            {
                bid.Estatus = "Rechazada";
                bid.Aceptada = false;
                bid.FechaRespuesta = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Oferta {request.Accion}da correctamente" });
        }

        // ================================================================
        // PUT: api/Marketplace/listings/{listingId}/approve
        // APROBAR PUBLICACIÓN (Solo Admin)
        // ================================================================
        [HttpPut("listings/{listingId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> ApproveListing(int listingId)
        {
            try
            {
                var listing = await _context.MarketplaceListings.FindAsync(listingId);

                if (listing == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });

                if (listing.Estatus != "Pendiente")
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Esta publicación ya fue procesada"
                    });

                listing.Estatus = "Activa";
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Publicación aprobada exitosamente",
                    Data = "Activa"
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

        // ================================================================
        // PUT: api/Marketplace/listings/{listingId}/reject
        // RECHAZAR PUBLICACIÓN (Solo Admin)
        // ================================================================
        [HttpPut("listings/{listingId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> RejectListing(int listingId)
        {
            try
            {
                var listing = await _context.MarketplaceListings.FindAsync(listingId);

                if (listing == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });

                if (listing.Estatus != "Pendiente")
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Esta publicación ya fue procesada"
                    });

                listing.Estatus = "Rechazada";
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Publicación rechazada",
                    Data = "Rechazada"
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

        // ================================================================
        // GET: api/Marketplace/listings/{listingId}/bids
        // OBTENER OFERTAS DE UNA PUBLICACIÓN (Vendedor o Admin)
        // ================================================================
        [HttpGet("listings/{listingId}/bids")]
        public async Task<ActionResult<ApiResponse<List<BidDto>>>> GetBidsForListing(int listingId)
        {
            try
            {
                var userId = GetUserId();
                var listing = await _context.MarketplaceListings.FindAsync(listingId);

                if (listing == null)
                    return NotFound(new ApiResponse<List<BidDto>>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });

                // ✅ PERMITIR AL VENDEDOR O AL ADMIN
                var isVendedor = listing.UserID_Vendedor == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isVendedor && !isAdmin)
                    return StatusCode(403, new ApiResponse<List<BidDto>>
                    {
                        Success = false,
                        Message = "No tienes permisos para ver estas ofertas"
                    });

                var bids = await _context.MarketplaceBids
                    .Include(b => b.Comprador)
                    .Where(b => b.ListingID == listingId)
                    .OrderByDescending(b => b.FechaOferta)
                    .Select(b => new BidDto
                    {
                        BidID = b.BidID,
                        CompradorID = b.UserID_Comprador,
                        CompradorNombre = b.Comprador.Nombre,
                        CompradorAvatar = b.Comprador.AvatarURL,
                        MontoOferta = b.MontoOferta,
                        Mensaje = b.Mensaje,
                        FechaOferta = b.FechaOferta,
                        Aceptada = b.Aceptada,
                        Estatus = b.Estatus,
                        FechaRespuesta = b.FechaRespuesta
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<BidDto>>
                {
                    Success = true,
                    Message = "Ofertas obtenidas",
                    Data = bids
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<BidDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}