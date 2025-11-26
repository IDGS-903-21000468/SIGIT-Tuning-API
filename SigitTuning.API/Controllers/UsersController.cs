using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using System.Security.Claims;
using BCrypt.Net;

namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UsersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Usuario";
        }

        private bool IsAdmin()
        {
            return GetUserRole() == "Admin";
        }

        // ===== GET: api/Users (Obtener todos los usuarios - SOLO ADMIN) =====
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<UserDetailDto>>>> GetAllUsers()
        {
            try
            {
                // Verificar que sea Admin
                if (!IsAdmin())
                {
                    return Unauthorized(new ApiResponse<List<UserDetailDto>>
                    {
                        Success = false,
                        Message = "Solo administradores pueden ver el listado de usuarios"
                    });
                }

                var users = await _context.Users
                    .Select(u => new UserDetailDto
                    {
                        UserID = u.UserID,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        Telefono = u.Telefono,
                        AvatarURL = u.AvatarURL,
                        FechaRegistro = u.FechaRegistro,
                        UltimaConexion = u.UltimaConexion,
                        Activo = u.Activo,
                        Rol = u.Rol
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<UserDetailDto>>
                {
                    Success = true,
                    Message = "Usuarios obtenidos exitosamente",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserDetailDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // ===== DELETE: api/Users/5 (Eliminar usuario - SOLO ADMIN) =====
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
        {
            try
            {
                var userId = GetUserId();

                // Admin puede eliminar cualquier usuario
                // Usuario normal solo puede eliminar su propia cuenta
                if (userId != id && !IsAdmin())
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No tienes permiso para eliminar este usuario"
                    });
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    });
                }

                user.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Usuario eliminado exitosamente"
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

        // ===== PUT: api/Users/5 (Actualizar usuario) =====
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDetailDto>>> UpdateUser(int id, [FromForm] UpdateUserDto request)
        {
            try
            {
                var userId = GetUserId();

                // Solo el usuario o un admin puede actualizar
                if (userId != id && !IsAdmin())
                {
                    return Unauthorized(new ApiResponse<UserDetailDto>
                    {
                        Success = false,
                        Message = "No tienes permiso para actualizar este usuario"
                    });
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserDetailDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    });
                }

                if (!string.IsNullOrEmpty(request.Nombre))
                    user.Nombre = request.Nombre;

                if (!string.IsNullOrEmpty(request.Telefono))
                    user.Telefono = request.Telefono;

                // Actualizar Rol (Solo si quien lo pide es Admin)
                if (!string.IsNullOrEmpty(request.Rol))
                {
                    // Validacion de seguridad: Solo un Admin puede cambiar roles.
                    if (IsAdmin())
                    {
                        user.Rol = request.Rol;
                    }
                }

                if (request.Avatar != null && request.Avatar.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(request.Avatar.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new ApiResponse<UserDetailDto>
                        {
                            Success = false,
                            Message = "Solo se permiten archivos de imagen"
                        });
                    }

                    if (request.Avatar.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new ApiResponse<UserDetailDto>
                        {
                            Success = false,
                            Message = "La imagen no debe exceder 5MB"
                        });
                    }

                    if (!string.IsNullOrEmpty(user.AvatarURL))
                    {
                        var oldFileName = Path.GetFileName(user.AvatarURL);
                        var oldFilePath = Path.Combine(_env.WebRootPath, "uploads", "avatars", oldFileName);

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "avatars");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Avatar.CopyToAsync(stream);
                    }

                    user.AvatarURL = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{uniqueFileName}";
                }

                await _context.SaveChangesAsync();

                var userDto = new UserDetailDto
                {
                    UserID = user.UserID,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    AvatarURL = user.AvatarURL,
                    FechaRegistro = user.FechaRegistro,
                    UltimaConexion = user.UltimaConexion,
                    Activo = user.Activo,
                    Rol = user.Rol
                };

                return Ok(new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Message = "Usuario actualizado exitosamente",
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}