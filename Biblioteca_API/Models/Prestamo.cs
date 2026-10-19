namespace Biblioteca_API.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public int SocioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public bool Devuelto {  get; set; }

    public Libro? Libro { get; set; }
    public Socio? Socio { get; set; }
}