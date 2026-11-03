using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Models;

// El índice único de DNI ya está definido en BibliotecaDbContext.
// El de Email se declara acá para no tocar el DbContext.
[Index(nameof(Email), IsUnique = true)]
public class Socio
{
    public int Id { get; set; }

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

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}