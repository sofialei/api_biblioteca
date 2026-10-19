public class Prestamo
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public int SocioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucíon { get; set; }
    public bool Devuelto {  get; set; }

    public Libro? Libro { get; set; }
    public Socio? Socio { get; set; }
}