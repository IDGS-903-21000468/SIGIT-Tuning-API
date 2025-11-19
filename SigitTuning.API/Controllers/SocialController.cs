using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using System.Security.Claims;
// NUEVO: Imports necesarios para la subida de archivos
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;


namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SocialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // NUEVO: Variable para saber dónde guardar los archivos
        private readonly IWebHostEnvironment _env;

        // MODIFICADO: Constructor ahora inyecta IWebHostEnvironment
        public SocialController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; // NUEVO
        }

        private int GetUserId()
        {
            // ... (Sin cambios)
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetTiempoTranscurrido(DateTime fecha)
        {
            // ... (Sin cambios)
            var timeSpan = DateTime.Now - fecha;

            if (timeSpan.TotalMinutes < 1) return "Ahora";
            if (timeSpan.TotalMinutes < 60) return $"Hace {(int)timeSpan.TotalMinutes}m";
            if (timeSpan.TotalHours < 24) return $"Hace {(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7) return $"Hace {(int)timeSpan.TotalDays}d";
            return fecha.ToString("dd/MM/yyyy");
        }

        // GET: api/Social/posts
        [HttpGet("posts")]
        public async Task<ActionResult<ApiResponse<List<SocialPostDto>>>> GetPosts()
        {
            // ... (Sin cambios)
            try
            {
                var userId = GetUserId();

                var posts = await _context.SocialPosts
                    .Include(p => p.Usuario)
                    .Include(p => p.Likes)
                    .Include(p => p.Comentarios)
                    .Where(p => p.Activo)
                    .OrderByDescending(p => p.FechaPublicacion)
                    .Select(p => new SocialPostDto
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UsuarioNombre = p.Usuario.Nombre,
                        UsuarioAvatar = p.Usuario.AvatarURL,
                        Titulo = p.Titulo,
                        Descripcion = p.Descripcion,
                        ImagenURL = p.ImagenURL,
                        FechaPublicacion = p.FechaPublicacion,
                        TotalLikes = p.Likes.Count,
                        TotalComentarios = p.Comentarios.Count,
                        UsuarioLeDioLike = p.Likes.Any(l => l.UserID == userId),
                        TiempoTranscurrido = ""
                    })
                    .ToListAsync();

                // Agregar tiempo transcurrido
                posts.ForEach(p => p.TiempoTranscurrido = GetTiempoTranscurrido(p.FechaPublicacion));

                return Ok(new ApiResponse<List<SocialPostDto>>
                {
                    Success = true,
                    Message = "Publicaciones obtenidas exitosamente",
                    Data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<SocialPostDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Social/posts
        [HttpPost("posts")]
        public async Task<ActionResult<ApiResponse<SocialPostDto>>> CreatePost(CreateSocialPostDto request)
        {
            // ... (Sin cambios)
            try
            {
                var userId = GetUserId();

                var post = new SocialPost
                {
                    UserID = userId,
                    Titulo = request.Titulo,
                    Descripcion = request.Descripcion,
                    ImagenURL = request.ImagenURL,
                    FechaPublicacion = DateTime.Now,
                    Activo = true
                };

                _context.SocialPosts.Add(post);
                await _context.SaveChangesAsync();

                var postDto = await _context.SocialPosts
                    .Include(p => p.Usuario)
                    .Where(p => p.PostID == post.PostID)
                    .Select(p => new SocialPostDto
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UsuarioNombre = p.Usuario.Nombre,
                        UsuarioAvatar = p.Usuario.AvatarURL,
                        Titulo = p.Titulo,
                        Descripcion = p.Descripcion,
                        ImagenURL = p.ImagenURL,
                        FechaPublicacion = p.FechaPublicacion,
                        TotalLikes = 0,
                        TotalComentarios = 0,
                        UsuarioLeDioLike = false,
                        TiempoTranscurrido = "Ahora"
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<SocialPostDto>
                {
                    Success = true,
                    Message = "Publicación creada exitosamente",
                    Data = postDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SocialPostDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        //
        // NUEVO: ENDPOINT PARA SUBIR LA IMAGEN
        //
        // POST: api/Social/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // 1. Validar que el archivo exista
            if (file == null || file.Length == 0)
            {
                // NOTA: Usamos ImageUploadResponseDto, no ApiResponse<T>
                return BadRequest(new ImageUploadResponseDto
                {
                    Success = false,
                    Message = "No se envió ningún archivo."
                });
            }

            // 2. Definir la ruta de guardado (en wwwroot/uploads/social)
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "social");

            // Asegurarse que el directorio exista
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // 3. Crear un nombre de archivo único
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // 4. Guardar el archivo en el servidor
            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ImageUploadResponseDto
                {
                    Success = false,
                    Message = $"Error interno al guardar: {ex.Message}"
                });
            }

            // 5. Construir la URL pública que se devolverá a la app
            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/social/{uniqueFileName}";

            // 6. Devolver la respuesta exitosa con la URL
            return Ok(new ImageUploadResponseDto
            {
                Success = true,
                Message = "Imagen subida exitosamente",
                Url = publicUrl // Esta es la URL que el ViewModel de Android recibirá
            });
        }


        // POST: api/Social/posts/5/like
        [HttpPost("posts/{postId}/like")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleLike(int postId)
        {
            // ... (Sin cambios)
            try
            {
                var userId = GetUserId();

                var existingLike = await _context.SocialLikes
                    .FirstOrDefaultAsync(l => l.PostID == postId && l.UserID == userId);

                if (existingLike != null)
                {
                    // Quitar like
                    _context.SocialLikes.Remove(existingLike);
                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Like removido"
                    });
                }
                else
                {
                    // Agregar like
                    var like = new SocialLike
                    {
                        PostID = postId,
                        UserID = userId,
                        FechaLike = DateTime.Now
                    };

                    _context.SocialLikes.Add(like);
                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Like agregado"
                    });
                }
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

        // GET: api/Social/posts/5/comments
        [HttpGet("posts/{postId}/comments")]
        public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetComments(int postId)
        {
            // ... (Sin cambios)
            try
            {
                var comments = await _context.SocialComments
                    .Include(c => c.Usuario)
                    .Where(c => c.PostID == postId)
                    .OrderBy(c => c.FechaComentario)
                    .Select(c => new CommentDto
                    {
                        CommentID = c.CommentID,
                        UserID = c.UserID,
                        UsuarioNombre = c.Usuario.Nombre,
                        UsuarioAvatar = c.Usuario.AvatarURL,
                        TextoComentario = c.TextoComentario,
                        FechaComentario = c.FechaComentario,
                        TiempoTranscurrido = ""
                    })
                    .ToListAsync();

                comments.ForEach(c => c.TiempoTranscurrido = GetTiempoTranscurrido(c.FechaComentario));

                return Ok(new ApiResponse<List<CommentDto>>
                {
                    Success = true,
                    Message = "Comentarios obtenidos exitosamente",
                    Data = comments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CommentDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/Social/posts/5/comments
        [HttpPost("posts/{postId}/comments")]
        public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(int postId, CreateCommentDto request)
        {
            // ... (Sin cambios)
            try
            {
                var userId = GetUserId();

                var comment = new SocialComment
                {
                    PostID = postId,
                    UserID = userId,
                    TextoComentario = request.TextoComentario,
                    FechaComentario = DateTime.Now
                };

                _context.SocialComments.Add(comment);
                await _context.SaveChangesAsync();

                var commentDto = await _context.SocialComments
                    .Include(c => c.Usuario)
                    .Where(c => c.CommentID == comment.CommentID)
                    .Select(c => new CommentDto
                    {
                        CommentID = c.CommentID,
                        UserID = c.UserID,
                        UsuarioNombre = c.Usuario.Nombre,
                        UsuarioAvatar = c.Usuario.AvatarURL,
                        TextoComentario = c.TextoComentario,
                        FechaComentario = c.FechaComentario,
                        TiempoTranscurrido = "Ahora"
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<CommentDto>
                {
                    Success = true,
                    Message = "Comentario agregado exitosamente",
                    Data = commentDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // DELETE: api/Social/posts/5
        [HttpDelete("posts/{postId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeletePost(int postId)
        {
            // ... (Sin cambios)
            try
            {
                var userId = GetUserId();

                var post = await _context.SocialPosts
                    .FirstOrDefaultAsync(p => p.PostID == postId && p.UserID == userId);

                if (post == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Publicación no encontrada o no tienes permiso para eliminarla"
                    });
                }

                post.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Publicación eliminada"
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