using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public DateTime? UltimaConexion { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(500)]
        public string? AvatarURL { get; set; }

        // Relaciones con otras tablas
        public virtual ICollection<ShoppingCartItem>? CarritoItems { get; set; }
        public virtual ICollection<Order>? Pedidos { get; set; }
        public virtual ICollection<MarketplaceListing>? PublicacionesMarketplace { get; set; }
        public virtual ICollection<SocialPost>? PublicacionesSociales { get; set; }
        public virtual ICollection<AssistantConsultation>? Consultas { get; set; }
    }
}