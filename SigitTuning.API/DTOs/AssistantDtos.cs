using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== CONSULTA AL ASISTENTE IA =====
    public class AssistantQueryDto
    {
        [Required(ErrorMessage = "La descripción del problema es requerida")]
        public string ProblemaDescrito { get; set; }

        public string? ImagenURL { get; set; }
    }

    // ===== RESPUESTA DEL ASISTENTE IA =====
    public class AssistantResponseDto
    {
        public bool Success { get; set; }
        public string RespuestaIA { get; set; }
        public List<ProductDto> ProductosSugeridos { get; set; } = new();
    }

    // ===== HISTORIAL DE CONSULTA =====
    public class ConsultationHistoryDto
    {
        public int ConsultationID { get; set; }
        public string ProblemaDescrito { get; set; }
        public string RespuestaIA { get; set; }
        public string? ImagenURL { get; set; }
        public DateTime FechaConsulta { get; set; }
        public int TotalProductosSugeridos { get; set; }
    }
}