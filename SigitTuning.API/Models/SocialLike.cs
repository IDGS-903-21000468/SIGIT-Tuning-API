using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("SocialLikes")]
    public class SocialLike
    {
        [Key]
        public int LikeID { get; set; }

        [Required]
        public int PostID { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime FechaLike { get; set; } = DateTime.Now;

        [ForeignKey("PostID")]
        public virtual SocialPost? Publicacion { get; set; }

        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }
    }
}