using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using System.Security.Claims;
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
        private readonly IWebHostEnvironment _env;

        public SocialController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetTiempoTranscurrido(DateTime fecha)
        {
            var timeSpan = DateTime.Now - fecha;

            if (timeSpan.TotalMinutes < 1) return "Ahora";
            if (timeSpan.TotalMinutes < 60) return $"Hace {(int)timeSpan.TotalMinutes}m";
            if (timeSpan.TotalHours < 24) return $"Hace {(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7) return $"Hace {(int)timeSpan.TotalDays}d";
            return fecha.ToString("dd/MM/yyyy");
        }

        // GET: api/Social/posts - MODIFICADO: Solo posts aprobados para usuarios normales
        [HttpGet("posts")]
        public async Task<ActionResult<ApiResponse<List<SocialPostDto>>>> GetPosts()
        {
            try
            {
                var userId = GetUserId();

                var posts = await _context.SocialPosts
                    .Include(p => p.Usuario)
                    .Include(p => p.Likes)
                    .Include(p => p.Comentarios)
                    .Where(p => p.Activo && p.Aprobado) // SOLO APROBADOS
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
                        TiempoTranscurrido = "",
                        Aprobado = p.Aprobado
                    })
                    .ToListAsync();

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

        // NUEVO: GET: api/Social/posts/pendientes - Para admins
        [HttpGet("posts/pendientes")]
        public async Task<ActionResult<ApiResponse<List<SocialPostDto>>>> GetPendingPosts()
        {
            try
            {
                var userId = GetUserId();

                var posts = await _context.SocialPosts
                    .Include(p => p.Usuario)
                    .Include(p => p.Likes)
                    .Include(p => p.Comentarios)
                    .Where(p => p.Activo && !p.Aprobado) // SOLO PENDIENTES
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
                        TiempoTranscurrido = "",
                        Aprobado = p.Aprobado
                    })
                    .ToListAsync();

                posts.ForEach(p => p.TiempoTranscurrido = GetTiempoTranscurrido(p.FechaPublicacion));

                return Ok(new ApiResponse<List<SocialPostDto>>
                {
                    Success = true,
                    Message = "Publicaciones pendientes obtenidas",
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

        // NUEVO: POST: api/Social/posts/5/aprobar
        [HttpPost("posts/{postId}/aprobar")]
        public async Task<ActionResult<ApiResponse<string>>> AprobarPost(int postId)
        {
            try
            {
                var userId = GetUserId();

                var post = await _context.SocialPosts.FindAsync(postId);

                if (post == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });
                }

                post.Aprobado = true;
                post.AprobadoPorUserID = userId;
                post.FechaAprobacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Publicación aprobada exitosamente"
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

        // NUEVO: POST: api/Social/posts/5/rechazar
        [HttpPost("posts/{postId}/rechazar")]
        public async Task<ActionResult<ApiResponse<string>>> RechazarPost(int postId)
        {
            try
            {
                var post = await _context.SocialPosts.FindAsync(postId);

                if (post == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Publicación no encontrada"
                    });
                }

                post.Activo = false; // Marcar como inactivo (rechazado)

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Publicación rechazada"
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

        // POST: api/Social/posts - MODIFICADO: Posts quedan pendientes por defecto
        [HttpPost("posts")]
        public async Task<ActionResult<ApiResponse<SocialPostDto>>> CreatePost(CreateSocialPostDto request)
        {
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
                    Activo = true,
                    Aprobado = false // PENDIENTE POR DEFECTO
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
                        TiempoTranscurrido = "Ahora",
                        Aprobado = p.Aprobado
                    })
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<SocialPostDto>
                {
                    Success = true,
                    Message = "Publicación enviada para aprobación",
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

        // POST: api/Social/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ImageUploadResponseDto
                {
                    Success = false,
                    Message = "No se envió ningún archivo."
                });
            }

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "social");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

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

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/social/{uniqueFileName}";

            return Ok(new ImageUploadResponseDto
            {
                Success = true,
                Message = "Imagen subida exitosamente",
                Url = publicUrl
            });
        }

        // POST: api/Social/posts/5/like
        [HttpPost("posts/{postId}/like")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleLike(int postId)
        {
            try
            {
                var userId = GetUserId();

                var existingLike = await _context.SocialLikes
                    .FirstOrDefaultAsync(l => l.PostID == postId && l.UserID == userId);

                if (existingLike != null)
                {
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