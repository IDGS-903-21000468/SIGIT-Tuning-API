using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public int ChatID { get; set; }

        [Required]
        public int SenderUserID { get; set; }

        [Required]
        public string Mensaje { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        public bool Leido { get; set; } = false;

        [ForeignKey("ChatID")]
        public virtual MarketplaceChat? Chat { get; set; }

        [ForeignKey("SenderUserID")]
        public virtual User? Remitente { get; set; }
    }
}