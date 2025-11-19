using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("MarketplaceChats")]
    public class MarketplaceChat
    {
        [Key]
        public int ChatID { get; set; }

        [Required]
        public int ListingID { get; set; }

        [Required]
        public int UserID_Vendedor { get; set; }

        [Required]
        public int UserID_Comprador { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        [ForeignKey("ListingID")]
        public virtual MarketplaceListing? Publicacion { get; set; }

        [ForeignKey("UserID_Vendedor")]
        public virtual User? Vendedor { get; set; }

        [ForeignKey("UserID_Comprador")]
        public virtual User? Comprador { get; set; }

        public virtual ICollection<ChatMessage>? Mensajes { get; set; }
    }
}
