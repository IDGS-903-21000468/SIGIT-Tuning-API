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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return int.Parse(userIdClaim);
        }

        // POST: api/Assistant/diagnose
        [HttpPost("diagnose")]
        public async Task<ActionResult<ApiResponse<AssistantResponseDto>>> DiagnoseIssue(AssistantQueryDto request)
        {
            try
            {
                var userId = GetUserId();

                // ✅ VERIFICAR QUE EL USUARIO EXISTE
                var userExists = await _context.Users.AnyAsync(u => u.UserID == userId);
                if (!userExists)
                {
                    return BadRequest(new ApiResponse<AssistantResponseDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    });
                }

                // GENERAR RESPUESTA ESPECIALIZADA EN TUNING
                var respuestaIA = GenerarRespuestaIA(request.ProblemaDescrito);

                // Buscar productos relevantes
                var productosSugeridos = await BuscarProductosRelevantes(request.ProblemaDescrito);

                // ✅ GUARDAR IDs de productos como JSON
                var productosIds = productosSugeridos.Select(p => p.ProductID).ToList();
                var productosJson = JsonSerializer.Serialize(productosIds);

                // ✅ GUARDAR EN BASE DE DATOS
                var consultation = new AssistantConsultation
                {
                    UserID = userId,
                    ImagenURL = request.ImagenURL,
                    ProblemaDescrito = request.ProblemaDescrito.Trim(),
                    RespuestaIA = respuestaIA,
                    ProductosSugeridos = productosJson,
                    FechaConsulta = DateTime.Now
                };

                _context.AssistantConsultations.Add(consultation);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<AssistantResponseDto>
                {
                    Success = true,
                    Message = $"Diagnóstico completado. Consulta guardada con ID: {consultation.ConsultationID}",
                    Data = new AssistantResponseDto
                    {
                        Success = true,
                        RespuestaIA = respuestaIA,
                        ProductosSugeridos = productosSugeridos
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<AssistantResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AssistantResponseDto>
                {
                    Success = false,
                    Message = $"Error al procesar diagnóstico: {ex.Message}"
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

                // ⚠️ TEMPORAL: Ver TODAS las consultas para debug
                var consultas = await _context.AssistantConsultations
                    // .Where(c => c.UserID == userId) // ← COMENTAR ESTA LÍNEA TEMPORALMENTE
                    .OrderByDescending(c => c.FechaConsulta)
                    .Take(50)
                    .ToListAsync();

                var historyDto = consultas.Select(c =>
                {
                    var productosIds = new List<int>();

                    if (!string.IsNullOrEmpty(c.ProductosSugeridos))
                    {
                        try
                        {
                            productosIds = JsonSerializer.Deserialize<List<int>>(c.ProductosSugeridos) ?? new List<int>();
                        }
                        catch
                        {
                            // Si falla deserialización, dejar vacío
                        }
                    }

                    return new ConsultationHistoryDto
                    {
                        ConsultationID = c.ConsultationID,
                        ProblemaDescrito = c.ProblemaDescrito,
                        RespuestaIA = c.RespuestaIA,
                        ImagenURL = c.ImagenURL,
                        FechaConsulta = c.FechaConsulta,
                        TotalProductosSugeridos = productosIds.Count
                    };
                }).ToList();

                return Ok(new ApiResponse<List<ConsultationHistoryDto>>
                {
                    Success = true,
                    Message = $"Historial obtenido: {historyDto.Count} consultas (UserID actual: {userId})",
                    Data = historyDto
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

        // ================================================================
        // MÉTODO: GENERAR RESPUESTA IA ESPECIALIZADA EN TUNING
        // ================================================================
        private string GenerarRespuestaIA(string problema)
        {
            var problemaLower = problema.ToLower();

            // ===== TUNING Y PERFORMANCE =====
            if (problemaLower.Contains("potencia") || problemaLower.Contains("hp") ||
                problemaLower.Contains("caballos") || problemaLower.Contains("más rápido") ||
                problemaLower.Contains("velocidad") || problemaLower.Contains("aceleración"))
            {
                return "🏁 **AUMENTAR POTENCIA Y PERFORMANCE**\n\n" +
                       "Para ganar potencia en tu vehículo, te recomiendo:\n\n" +
                       "1️⃣ **Sistema de admisión de aire frío** (Cold Air Intake) - Mejora el flujo de aire\n" +
                       "2️⃣ **Sistema de escape deportivo** - Reduce la contrapresión\n" +
                       "3️⃣ **Reprogramación ECU** (Chip tuning) - Optimiza mapas de inyección\n" +
                       "4️⃣ **Turbo o Supercargador** - Para ganancias significativas\n" +
                       "5️⃣ **Intercooler mejorado** - Si tienes turbo\n\n" +
                       "💡 **Tip Pro**: Empieza con intake y escape, luego reprograma ECU para aprovechar al máximo.\n\n" +
                       "Aquí tienes productos de performance disponibles:";
            }

            // ===== SUSPENSIÓN Y MANEJO =====
            if (problemaLower.Contains("suspension") || problemaLower.Contains("coilover") ||
                problemaLower.Contains("amortiguador") || problemaLower.Contains("rebota") ||
                problemaLower.Contains("manejo") || problemaLower.Contains("altura") ||
                problemaLower.Contains("bajar"))
            {
                return "🔧 **SUSPENSIÓN Y TUNING DE CHASIS**\n\n" +
                       "Para mejorar el manejo y stance de tu auto:\n\n" +
                       "1️⃣ **Coilovers ajustables** - Control total de altura y dureza\n" +
                       "2️⃣ **Springs deportivos** - Opción económica para bajar\n" +
                       "3️⃣ **Barras estabilizadoras** - Reduce roll en curvas\n" +
                       "4️⃣ **Bujes de poliuretano** - Mayor respuesta del chasis\n" +
                       "5️⃣ **Strut bars** - Rigidez estructural\n\n" +
                       "💡 **Tip Pro**: Los coilovers son la mejor inversión a largo plazo.\n\n" +
                       "Productos de suspensión disponibles:";
            }

            // ===== FRENOS Y SEGURIDAD =====
            if (problemaLower.Contains("freno") || problemaLower.Contains("frenar") ||
                problemaLower.Contains("pastilla") || problemaLower.Contains("disco") ||
                problemaLower.Contains("brake"))
            {
                return "🛑 **SISTEMA DE FRENOS DE ALTA PERFORMANCE**\n\n" +
                       "Para mejorar tu sistema de frenado:\n\n" +
                       "1️⃣ **Pastillas de alto rendimiento** - Mejor fricción y menos fade\n" +
                       "2️⃣ **Discos perforados/ranurados** - Mejor disipación de calor\n" +
                       "3️⃣ **Líquido de frenos DOT 4/5.1** - Punto de ebullición más alto\n" +
                       "4️⃣ **Líneas de freno de acero** - Mejor feeling del pedal\n" +
                       "5️⃣ **Kit Big Brake** - Para track days\n\n" +
                       "💡 **Tip Pro**: Cambia pastillas y líquido primero, luego discos.\n\n" +
                       "Productos de frenado disponibles:";
            }

            // ===== ESCAPE Y SONIDO =====
            if (problemaLower.Contains("escape") || problemaLower.Contains("mofle") ||
                problemaLower.Contains("sonido") || problemaLower.Contains("ruidoso") ||
                problemaLower.Contains("exhaust") || problemaLower.Contains("silenciador"))
            {
                return "🔊 **SISTEMA DE ESCAPE DEPORTIVO**\n\n" +
                       "Para mejorar rendimiento y sonido:\n\n" +
                       "1️⃣ **Cat-back exhaust** - Sistema completo desde catalizador\n" +
                       "2️⃣ **Axle-back** - Solo mofle trasero (más económico)\n" +
                       "3️⃣ **Headers/Manifold** - Mejora flujo desde motor\n" +
                       "4️⃣ **High-flow catalytic converter** - Mantiene emisiones legales\n" +
                       "5️⃣ **Downpipe** - Para autos turbo\n\n" +
                       "💡 **Tip Pro**: Un sistema cat-back es el mejor balance precio/rendimiento.\n\n" +
                       "Productos de escape disponibles:";
            }

            // ===== LLANTAS Y RINES =====
            if (problemaLower.Contains("llanta") || problemaLower.Contains("rin") ||
                problemaLower.Contains("rueda") || problemaLower.Contains("wheel") ||
                problemaLower.Contains("tire") || problemaLower.Contains("agarre"))
            {
                return "🛞 **LLANTAS Y RINES - TUNING VISUAL Y PERFORMANCE**\n\n" +
                       "Para mejorar look y agarre:\n\n" +
                       "1️⃣ **Rines ligeros de aleación** - Reduce peso no suspendido\n" +
                       "2️⃣ **Llantas de alto rendimiento** - Mejor agarre en curvas\n" +
                       "3️⃣ **Set staggered** - Rines anchos atrás para RWD\n" +
                       "4️⃣ **Spacers de rueda** - Para flush fitment\n" +
                       "5️⃣ **Lug nuts/locks** - Protección contra robo\n\n" +
                       "💡 **Tip Pro**: Prioriza calidad sobre diseño. Rines forjados > Cast.\n\n" +
                       "Productos de llantas y rines disponibles:";
            }

            // ===== ILUMINACIÓN Y LED =====
            if (problemaLower.Contains("luz") || problemaLower.Contains("led") ||
                problemaLower.Contains("faro") || problemaLower.Contains("iluminación") ||
                problemaLower.Contains("headlight") || problemaLower.Contains("xenon"))
            {
                return "💡 **ILUMINACIÓN LED Y TUNING VISUAL**\n\n" +
                       "Mejora la visibilidad y estética:\n\n" +
                       "1️⃣ **LED Headlights** - Mejor visibilidad nocturna\n" +
                       "2️⃣ **Angel eyes/Halos** - Look agresivo\n" +
                       "3️⃣ **Underglow LED strips** - Iluminación inferior\n" +
                       "4️⃣ **Tail lights LED** - Moderniza la parte trasera\n" +
                       "5️⃣ **Interior LED kit** - Ambiente personalizado\n\n" +
                       "💡 **Tip Pro**: Verifica la legalidad de las modificaciones en tu estado.\n\n" +
                       "Productos de iluminación disponibles:";
            }

            // ===== AERODINÁMICA =====
            if (problemaLower.Contains("aerodinamico") || problemaLower.Contains("spoiler") ||
                problemaLower.Contains("aleron") || problemaLower.Contains("difusor") ||
                problemaLower.Contains("bodykit") || problemaLower.Contains("splitter"))
            {
                return "✈️ **AERODINÁMICA Y BODY KITS**\n\n" +
                       "Para mejorar downforce y estética:\n\n" +
                       "1️⃣ **Rear spoiler/wing** - Aumenta estabilidad a altas velocidades\n" +
                       "2️⃣ **Front splitter** - Reduce lift frontal\n" +
                       "3️⃣ **Side skirts** - Canaliza flujo de aire\n" +
                       "4️⃣ **Rear diffuser** - Acelera flujo bajo el auto\n" +
                       "5️⃣ **Hood vents** - Extrae calor del motor\n\n" +
                       "💡 **Tip Pro**: Aero funcional > Estético. Prioriza downforce real.\n\n" +
                       "Productos aerodinámicos disponibles:";
            }

            // ===== INTERIOR Y GAUGES =====
            if (problemaLower.Contains("interior") || problemaLower.Contains("gauge") ||
                problemaLower.Contains("asiento") || problemaLower.Contains("volante") ||
                problemaLower.Contains("racing seat") || problemaLower.Contains("boost"))
            {
                return "🏎️ **INTERIOR RACING Y GAUGES**\n\n" +
                       "Personaliza tu cabina:\n\n" +
                       "1️⃣ **Racing seats** - Mejor soporte en curvas\n" +
                       "2️⃣ **Volante deportivo** - Mejor agarre y control\n" +
                       "3️⃣ **Harness de 4/6 puntos** - Seguridad en track\n" +
                       "4️⃣ **Gauges digitales** - Monitorea boost, AFR, temp\n" +
                       "5️⃣ **Shift knob** - Cambios más precisos\n\n" +
                       "💡 **Tip Pro**: Los gauges son esenciales para monitorear el motor.\n\n" +
                       "Productos de interior disponibles:";
            }

            // ===== DEFAULT: TUNING GENERAL =====
            return "🔰 **ASISTENTE DE TUNING SIGIT**\n\n" +
                   "Soy tu especialista en modificaciones automotrices. Puedo ayudarte con:\n\n" +
                   "⚡ **Performance**: Potencia, turbo, ECU tuning\n" +
                   "🔧 **Suspensión**: Coilovers, stance, manejo\n" +
                   "🛑 **Frenos**: Sistemas de alto rendimiento\n" +
                   "🔊 **Escape**: Sonido y rendimiento\n" +
                   "🛞 **Llantas/Rines**: Fitment y performance\n" +
                   "✈️ **Aero**: Bodykits, spoilers, difusores\n" +
                   "💡 **Iluminación**: LED, xenon, underglow\n" +
                   "🏎️ **Interior**: Racing seats, gauges, volantes\n\n" +
                   "¿Qué modificación tienes en mente? Cuéntame sobre tu auto y qué quieres lograr.\n\n" +
                   "Mientras, aquí tienes productos populares:";
        }

        // ================================================================
        // MÉTODO: BUSCAR PRODUCTOS RELEVANTES
        // ================================================================
        private async Task<List<ProductDto>> BuscarProductosRelevantes(string problema)
        {
            var problemaLower = problema.ToLower();
            var palabrasClave = new List<string>();

            // Detectar categorías
            if (problemaLower.Contains("freno") || problemaLower.Contains("brake"))
                palabrasClave.Add("Frenos");

            if (problemaLower.Contains("llanta") || problemaLower.Contains("rin") ||
                problemaLower.Contains("wheel") || problemaLower.Contains("tire"))
                palabrasClave.Add("Llantas");

            if (problemaLower.Contains("suspension") || problemaLower.Contains("coilover") ||
                problemaLower.Contains("amortiguador"))
                palabrasClave.Add("Suspensión");

            if (problemaLower.Contains("escape") || problemaLower.Contains("mofle") ||
                problemaLower.Contains("exhaust"))
                palabrasClave.Add("Escape");

            if (problemaLower.Contains("aerodinamico") || problemaLower.Contains("spoiler") ||
                problemaLower.Contains("bodykit"))
                palabrasClave.Add("Aerodinámica");

            // Si no hay palabras clave, retornar productos populares
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

            // ✅ PRIMERO: Obtener TODOS los productos activos con categoría
            var todosLosProductos = await _context.Products
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0)
                .ToListAsync();

            // ✅ SEGUNDO: Filtrar EN MEMORIA por categorías
            var productosFiltrados = todosLosProductos
                .Where(p => palabrasClave.Contains(p.Categoria.Nombre))
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
                .ToList();

            // Si no encontró productos de esas categorías, devolver populares
            if (!productosFiltrados.Any())
            {
                return todosLosProductos
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
                    .ToList();
            }

            return productosFiltrados;
        }
    }
}