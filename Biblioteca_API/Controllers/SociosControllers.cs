using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_API.Controller;

[ApiController]
[Route("api/[controller]")]
public class SociosController : ControllerBase
{
    private readonly ISocioService _socioService;

    public SociosController(ISocioService socioService)
    {
        _socioService = socioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SocioResponseDTO>>> ObtenerTodos()
        => Ok(await _socioService.ObtenerTodosAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SocioResponseDTO>> ObtenerPorId(int id)
    {
        var socio = await _socioService.ObtenerPorIdAsync(id);
        return socio is null ? NotFound() : Ok(socio);
    }

    [HttpPost]
    public async Task<ActionResult<SocioResponseDTO>> Crear(SocioDTO dto)
    {
        try
        {
            var creado = await _socioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, SocioDTO dto)
    {
        try
        {
            return await _socioService.ActualizarAsync(id, dto) ? NoContent() : NotFound();
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
            return await _socioService.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
