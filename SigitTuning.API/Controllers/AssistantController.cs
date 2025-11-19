using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssistantController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // POST: api/Assistant/diagnose
        [HttpPost("diagnose")]
        public async Task<ActionResult<ApiResponse<AssistantResponseDto>>> DiagnoseIssue(AssistantQueryDto request)
        {
            try
            {
                var userId = GetUserId();

                // SIMULACIÓN DE RESPUESTA DE IA
                // En producción, aquí llamarías a tu servicio de IA (Gemini, OpenAI, etc.)
                var respuestaIA = GenerarRespuestaSimulada(request.ProblemaDescrito);

                // Buscar productos sugeridos basados en palabras clave
                var productosSugeridos = await BuscarProductosRelevantes(request.ProblemaDescrito);

                // Guardar consulta en historial
                var consultation = new AssistantConsultation
                {
                    UserID = userId,
                    ImagenURL = request.ImagenURL,
                    ProblemaDescrito = request.ProblemaDescrito,
                    RespuestaIA = respuestaIA,
                    ProductosSugeridos = JsonSerializer.Serialize(productosSugeridos.Select(p => p.ProductID).ToList()),
                    FechaConsulta = DateTime.Now
                };

                _context.AssistantConsultations.Add(consultation);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<AssistantResponseDto>
                {
                    Success = true,
                    Message = "Diagnóstico completado",
                    Data = new AssistantResponseDto
                    {
                        Success = true,
                        RespuestaIA = respuestaIA,
                        ProductosSugeridos = productosSugeridos
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AssistantResponseDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/Assistant/history
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ConsultationHistoryDto>>>> GetHistory()
        {
            try
            {
                var userId = GetUserId();

                var history = await _context.AssistantConsultations
                    .Where(c => c.UserID == userId)
                    .OrderByDescending(c => c.FechaConsulta)
                    .Select(c => new ConsultationHistoryDto
                    {
                        ConsultationID = c.ConsultationID,
                        ProblemaDescrito = c.ProblemaDescrito,
                        RespuestaIA = c.RespuestaIA,
                        ImagenURL = c.ImagenURL,
                        FechaConsulta = c.FechaConsulta,
                        TotalProductosSugeridos = 0
                    })
                    .Take(20)
                    .ToListAsync();

                return Ok(new ApiResponse<List<ConsultationHistoryDto>>
                {
                    Success = true,
                    Message = "Historial obtenido exitosamente",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ConsultationHistoryDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // MÉTODOS AUXILIARES

        private string GenerarRespuestaSimulada(string problema)
        {
            var problemaLower = problema.ToLower();

            if (problemaLower.Contains("freno") || problemaLower.Contains("frenos"))
            {
                return "Basándome en tu descripción, parece ser un problema con el sistema de frenado. Te recomiendo revisar:\n\n" +
                       "1. Pastillas de freno (pueden estar desgastadas)\n" +
                       "2. Discos de freno (verificar si tienen ranuras o están cristalizados)\n" +
                       "3. Líquido de frenos (revisar nivel y color)\n\n" +
                       "A continuación te muestro productos disponibles que podrían ayudarte:";
            }
            else if (problemaLower.Contains("llanta") || problemaLower.Contains("rueda") || problemaLower.Contains("rin"))
            {
                return "Por lo que describes, tu problema está relacionado con las llantas o rines. Te sugiero:\n\n" +
                       "1. Revisar la presión de aire en todas las llantas\n" +
                       "2. Verificar el balance y alineación\n" +
                       "3. Inspeccionar si hay daños en los rines\n\n" +
                       "Aquí tienes algunos productos que pueden ser de utilidad:";
            }
            else if (problemaLower.Contains("suspension") || problemaLower.Contains("amortiguador"))
            {
                return "Tu problema parece estar en el sistema de suspensión. Considera:\n\n" +
                       "1. Amortiguadores desgastados (si el auto rebota mucho)\n" +
                       "2. Coilovers o resortes fatigados\n" +
                       "3. Bujes o rótulas con holgura\n\n" +
                       "Te recomiendo estos productos de suspensión:";
            }
            else if (problemaLower.Contains("escape") || problemaLower.Contains("mofle") || problemaLower.Contains("ruido motor"))
            {
                return "Según tu descripción, el problema está en el sistema de escape. Revisa:\n\n" +
                       "1. Fugas en el múltiple o tubería de escape\n" +
                       "2. Mofle o silenciador dañado\n" +
                       "3. Juntas o empaques deteriorados\n\n" +
                       "Aquí hay productos de escape que podrían ayudarte:";
            }
            else
            {
                return "Entiendo tu problema. Te recomiendo que un mecánico especializado revise tu vehículo para un diagnóstico más preciso. " +
                       "Mientras tanto, aquí tienes algunos productos populares que podrían ser útiles:";
            }
        }

        private async Task<List<ProductDto>> BuscarProductosRelevantes(string problema)
        {
            var problemaLower = problema.ToLower();
            var palabrasClave = new List<string>();

            // Detectar categorías relevantes
            if (problemaLower.Contains("freno"))
                palabrasClave.Add("Frenos");
            if (problemaLower.Contains("llanta") || problemaLower.Contains("rin"))
                palabrasClave.Add("Llantas");
            if (problemaLower.Contains("suspension") || problemaLower.Contains("amortiguador"))
                palabrasClave.Add("Suspensión");
            if (problemaLower.Contains("escape") || problemaLower.Contains("mofle"))
                palabrasClave.Add("Escape");

            // Si no se encontraron palabras clave, buscar productos populares
            if (!palabrasClave.Any())
            {
                return await _context.Products
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo && p.Stock > 0)
                    .OrderByDescending(p => p.Stock)
                    .Take(5)
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
            }

            // Buscar productos por categorías relevantes
            return await _context.Products
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0 && palabrasClave.Contains(p.Categoria.Nombre))
                .Take(5)
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
        }
    }
}