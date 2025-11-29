using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SigitTuning.API.DTOs
{
    // ===== CATEGORÍA DE PRODUCTO =====
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenURL { get; set; }
    }

    // ===== CREAR/ACTUALIZAR CATEGORÍA (ADMIN) =====
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        // URL de imagen si se manda un link
        public string? ImagenURL { get; set; }

        // Archivo de imagen si se sube desde el dispositivo
        public IFormFile? Imagen { get; set; }
    }

    // ===== PRODUCTO =====
    public class ProductDto
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string CategoriaNombre { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenURL { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Anio { get; set; }
        public bool Disponible => Stock > 0;
    }

    // ===== CREAR/ACTUALIZAR PRODUCTO (ADMIN) =====
    public class CreateProductDto
    {
        [Required]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        public string? ImagenURL { get; set; }
        public IFormFile? Imagen { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        [MaxLength(50)]
        public string? Anio { get; set; }
    }

    // ===== RESPUESTA GENÉRICA =====
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}