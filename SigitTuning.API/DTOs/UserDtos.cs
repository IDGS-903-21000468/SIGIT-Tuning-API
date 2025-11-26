using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== OBTENER USUARIO DETALLADO =====
    public class UserDetailDto
    {
        public int UserID { get; set; }

        [Required]
        public required string Nombre { get; set; }

        [Required]
        public required string Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public string? AvatarURL { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimaConexion { get; set; }

        public bool Activo { get; set; }

        public string Rol { get; set; } = "Usuario"; // ← NUEVO
    }

    // ===== ACTUALIZAR USUARIO CON IMAGEN =====
    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        // 👇 ¡TE FALTABA ESTA LÍNEA! 👇
        public string? Rol { get; set; }

        public IFormFile? Avatar { get; set; }
    }

    // ===== CAMBIAR CONTRASEÑA =====
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La contraseña antigua es requerida")]
        public required string PasswordAntigua { get; set; }

        [Required(ErrorMessage = "La contraseña nueva es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public required string PasswordNueva { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña nueva")]
        public required string PasswordNuevaConfirmacion { get; set; }
    }
}