using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Data;
using SigitTuning.API.DTOs;
using SigitTuning.API.Models;
using SigitTuning.API.Helpers;
using BCrypt.Net;

namespace SigitTuning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
        {
            try
            {
                // Validar si el email ya existe
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "El email ya está registrado"
                    });
                }

                // Crear el nuevo usuario
                var user = new User
                {
                    Nombre = request.Nombre,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Telefono = request.Telefono,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generar token JWT
                var jwtHelper = new JwtHelper(_configuration);
                var token = jwtHelper.GenerarToken(user.UserID, user.Email, user.Nombre);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Usuario registrado exitosamente",
                    Token = token,
                    Usuario = new UserDto
                    {
                        UserID = user.UserID,
                        Nombre = user.Nombre,
                        Email = user.Email,
                        Telefono = user.Telefono,
                        AvatarURL = user.AvatarURL,
                        FechaRegistro = user.FechaRegistro
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = $"Error al registrar usuario: {ex.Message}"
                });
            }
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            try
            {
                // Buscar usuario por email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email o contraseña incorrectos"
                    });
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email o contraseña incorrectos"
                    });
                }

                // Verificar si el usuario está activo
                if (!user.Activo)
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Usuario inactivo. Contacta al administrador"
                    });
                }

                // Actualizar última conexión
                user.UltimaConexion = DateTime.Now;
                await _context.SaveChangesAsync();

                // Generar token JWT
                var jwtHelper = new JwtHelper(_configuration);
                var token = jwtHelper.GenerarToken(user.UserID, user.Email, user.Nombre);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    Usuario = new UserDto
                    {
                        UserID = user.UserID,
                        Nombre = user.Nombre,
                        Email = user.Email,
                        Telefono = user.Telefono,
                        AvatarURL = user.AvatarURL,
                        FechaRegistro = user.FechaRegistro
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = $"Error en el login: {ex.Message}"
                });
            }
        }
    }
}