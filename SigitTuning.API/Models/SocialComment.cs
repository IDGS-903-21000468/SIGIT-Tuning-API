using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("SocialComments")]
    public class SocialComment
    {
        [Key]
        public int CommentID { get; set; }

        [Required]
        public int PostID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public string TextoComentario { get; set; }

        public DateTime FechaComentario { get; set; } = DateTime.Now;

        [ForeignKey("PostID")]
        public virtual SocialPost? Publicacion { get; set; }

        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }
    }
}