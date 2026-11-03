using System.ComponentModel.DataAnnotations;

namespace Biblioteca_API.DTOs;

// DTO de entrada (POST / PUT). Las validaciones las aplica automáticamente
// [ApiController] devolviendo 400 si algo falta o es inválido.
public class SocioDTO
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos, sin puntos.")]
    [StringLength(15)]
    public string DNI { get; set; } = string.Empty;
}

// DTO de salida: nunca se devuelven entidades del dominio directamente.
public class SocioResponseDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
}
