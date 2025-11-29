using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("SocialPosts")]
    public class SocialPost
    {
        [Key]
        public int PostID { get; set; }

        [Required]
        public int UserID { get; set; }

        [MaxLength(200)]
        public string? Titulo { get; set; }

        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }

        public virtual ICollection<SocialLike>? Likes { get; set; }
        public virtual ICollection<SocialComment>? Comentarios { get; set; }
        // En tu modelo SocialPost.cs, agrega:
        public bool Aprobado { get; set; } = false;
        public int? AprobadoPorUserID { get; set; }
        public DateTime? FechaAprobacion { get; set; }
    }
}
