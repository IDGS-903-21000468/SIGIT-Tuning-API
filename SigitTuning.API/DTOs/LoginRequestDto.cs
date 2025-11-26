using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== LOGIN =====
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }

    // ===== REGISTRO =====
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }
    }

    // ===== RESPUESTA DE LOGIN/REGISTRO =====
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public UserDto? Usuario { get; set; }
    }

    // ===== INFORMACIÓN DEL USUARIO =====
    // ===== INFORMACIÓN DEL USUARIO =====
    public class UserDto
    {
        public int UserID { get; set; }

        // Agregué 'required' para quitar las advertencias amarillas
        public required string Nombre { get; set; }
        public required string Email { get; set; }

        public string? Telefono { get; set; }
        public string? AvatarURL { get; set; }
        public DateTime FechaRegistro { get; set; }

        // 👇 ESTA ES LA LÍNEA QUE TE FALTABA 👇
        public string Rol { get; set; }
    }
}