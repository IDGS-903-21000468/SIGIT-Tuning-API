using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigitTuning.API.Models
{
    [Table("AssistantConsultations")]
    public class AssistantConsultation
    {
        [Key]
        public int ConsultationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [MaxLength(500)]
        public string? ImagenURL { get; set; }

        public string? ProblemaDescrito { get; set; }

        public string? RespuestaIA { get; set; }

        public string? ProductosSugeridos { get; set; }

        public DateTime FechaConsulta { get; set; } = DateTime.Now;

        [ForeignKey("UserID")]
        public virtual User? Usuario { get; set; }
    }
}