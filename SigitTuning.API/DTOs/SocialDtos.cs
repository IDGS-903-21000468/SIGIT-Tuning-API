using System.ComponentModel.DataAnnotations;

namespace SigitTuning.API.DTOs
{
    // ===== CREAR PUBLICACIÓN SOCIAL =====
    public class CreateSocialPostDto
    {
        [MaxLength(200)]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; }

        public string? ImagenURL { get; set; }
    }

    // ===== PUBLICACIÓN SOCIAL =====
    public class SocialPostDto
    {
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string UsuarioNombre { get; set; }
        public string? UsuarioAvatar { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenURL { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComentarios { get; set; }
        public bool UsuarioLeDioLike { get; set; }
        public string TiempoTranscurrido { get; set; }
        public bool Aprobado { get; set; } // NUEVO
    }

    // ===== CREAR COMENTARIO =====
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "El comentario no puede estar vacío")]
        public string TextoComentario { get; set; }
    }

    // ===== COMENTARIO =====
    public class CommentDto
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public string UsuarioNombre { get; set; }
        public string? UsuarioAvatar { get; set; }
        public string TextoComentario { get; set; }
        public DateTime FechaComentario { get; set; }
        public string TiempoTranscurrido { get; set; }
    }

   
}