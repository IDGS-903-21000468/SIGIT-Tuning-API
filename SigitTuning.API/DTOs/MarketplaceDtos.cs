using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== CREAR PUBLICACIÓN DE VENTA =====
    public class CreateMarketplaceListingDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [MaxLength(200)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; }

        public string? ImagenURL { get; set; }

        [Required(ErrorMessage = "El precio inicial es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioInicial { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        [Range(1900, 2100)]
        public int? Anio { get; set; }

        [Range(0, int.MaxValue)]
        public int? Kilometraje { get; set; }

        public string? Modificaciones { get; set; }
    }

    // ===== PUBLICACIÓN DE MARKETPLACE =====
    public class MarketplaceListingDto
    {
        public int ListingID { get; set; }
        public int VendedorID { get; set; }
        public string VendedorNombre { get; set; }
        public string? VendedorAvatar { get; set; }
        public string Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenURL { get; set; }
        public decimal PrecioInicial { get; set; }
        public decimal PrecioActual { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public int? Kilometraje { get; set; }
        public string? Modificaciones { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string Estatus { get; set; }
        public int TotalOfertas { get; set; }
        public decimal? MejorOferta { get; set; }
    }

    // ===== CREAR OFERTA =====
    public class CreateBidDto
    {
        [Required(ErrorMessage = "El monto de la oferta es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoOferta { get; set; }

        [MaxLength(500)]
        public string? Mensaje { get; set; }
    }

    // ===== OFERTA =====
    public class BidDto
    {
        public int BidID { get; set; }
        public int CompradorID { get; set; }
        public string CompradorNombre { get; set; }
        public string? CompradorAvatar { get; set; }
        public decimal MontoOferta { get; set; }
        public string? Mensaje { get; set; }
        public DateTime FechaOferta { get; set; }
        public bool Aceptada { get; set; }

        // ⚠️ NUEVOS CAMPOS
        public string Estatus { get; set; } = "Pendiente"; // "Pendiente", "Aceptada", "Rechazada"
        public DateTime? FechaRespuesta { get; set; }
    }

    // ⚠️ DTO para responder ofertas
    public class RespondBidRequest
    {
        [Required]
        public string Accion { get; set; } // "aceptar" o "rechazar"
    }

    public class RespondBidResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // ===== MENSAJE DEL CHAT =====
    public class CreateChatMessageDto
    {
        [Required(ErrorMessage = "El mensaje no puede estar vacío")]
        public string Mensaje { get; set; }
    }

    // ===== CHAT MESSAGE DTO =====
    public class ChatMessageDto
    {
        public int MessageID { get; set; }
        public int SenderUserID { get; set; }
        public string SenderNombre { get; set; }
        public string? SenderAvatar { get; set; }
        public string Mensaje { get; set; }
        public string FechaEnvio { get; set; } // String para evitar problemas de serialización
        public bool EsPropio { get; set; }
    }

    // ===== CHAT DTO =====
    public class ChatDto
    {
        public int ChatID { get; set; }
        public int ListingID { get; set; }
        public string ListingTitulo { get; set; }
        public string? ListingImagen { get; set; }
        public string ListingEstatus { get; set; } // Para saber si está "Activa" o "Vendida"
        public int OtroUsuarioID { get; set; }
        public string OtroUsuarioNombre { get; set; }
        public string? OtroUsuarioAvatar { get; set; }
        public bool SoyVendedor { get; set; } // Para mostrar botón de completar venta
        public List<ChatMessageDto> Mensajes { get; set; } = new();
    }

    // ===== CHAT SUMMARY (INBOX) =====
    public class ChatSummaryDto
    {
        public int ChatID { get; set; }
        public int ListingID { get; set; }
        public string ListingTitulo { get; set; }
        public string? ListingImagen { get; set; }
        public int OtroUsuarioID { get; set; }
        public string OtroUsuarioNombre { get; set; }
        public string? OtroUsuarioAvatar { get; set; }
        public string? UltimoMensaje { get; set; }
        public string? FechaUltimoMensaje { get; set; }
    }

    // ===== 🆕 COMPLETAR VENTA =====
    public class CompleteSaleDto
    {
        [Required(ErrorMessage = "El ID del comprador es requerido")]
        public int CompradorID { get; set; }
    }

    public class CompleteSaleResponseDto
    {
        public int ListingID { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Comision { get; set; }
        public decimal PrecioFinal { get; set; }
        public string CompradorNombre { get; set; }
        public DateTime FechaVenta { get; set; }
    }
}