using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_API.Controller;

[ApiController]
[Route("api/[controller]")]
public class LibrosController : ControllerBase
{
    private readonly ILibroService _libroService;

    public LibrosController(ILibroService libroService)
    {
        _libroService = libroService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LibroResponseDTO>>> ObtenerTodos()
        => Ok(await _libroService.ObtenerTodosAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibroResponseDTO>> ObtenerPorId(int id)
    {
        var libro = await _libroService.ObtenerPorIdAsync(id);
        return libro is null ? NotFound() : Ok(libro);
    }

    [HttpPost]
    public async Task<ActionResult<LibroResponseDTO>> Crear(LibroDTO dto)
    {
        try
        {
            var creado = await _libroService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, LibroDTO dto)
    {
        try
        {
            return await _libroService.ActualizarAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await _libroService.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
