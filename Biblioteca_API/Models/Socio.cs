public class Socio
{
    public int Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}