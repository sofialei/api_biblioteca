using System.ComponentModel.DataAnnotations;

namespace Biblioteca_API.DTOs;

public class PrestamoDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El LibroId es obligatorio")]
    public int LibroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El SocioId es obligatorio")]
    public int SocioId { get; set; }
}

public class PrestamoResponseDTO
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string TituloLibro { get; set; } = string.Empty;
    public int SocioId { get; set; }
    public string NombreSocio { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public bool Devuelto { get; set; }
}
