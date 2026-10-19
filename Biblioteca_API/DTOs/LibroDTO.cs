using System.ComponentModel.DataAnnotations;

namespace Biblioteca_API.DTOs;

public class LibroDTO
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio")]
    [StringLength(150)]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ISBN es obligatorio")]
    [RegularExpression("^[0-9]{10}([0-9]{3})?$",
        ErrorMessage = "El ISBN debe tener 10 o 13 dígitos, sin guiones")]
    public string ISBN { get; set; } = string.Empty;

    [StringLength(100)]
    public string Genero { get; set; } = string.Empty;
}

public class LibroResponseDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}