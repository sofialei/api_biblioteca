using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_API.Controller;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly IPrestamoService _prestamoService;

    public PrestamosController(IPrestamoService prestamoService)
    {
        _prestamoService = prestamoService;
    }

    /// <summary>Registra un préstamo (socio + libro).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrestamoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PrestamoResponseDTO>> Registrar(PrestamoDTO dto)
    {
        try
        {
            var creado = await _prestamoService.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>Registra la devolución de un préstamo.</summary>
    [HttpPut("{id:int}/devolucion")]
    [ProducesResponseType(typeof(PrestamoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PrestamoResponseDTO>> Devolver(int id)
    {
        try
        {
            return Ok(await _prestamoService.DevolverAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista los préstamos activos (no devueltos).</summary>
    [HttpGet("activos")]
    public async Task<ActionResult<List<PrestamoResponseDTO>>> ObtenerActivos()
        => Ok(await _prestamoService.ObtenerActivosAsync());

    /// <summary>Historial de préstamos de un socio.</summary>
    // El "~/" evita depender del SociosController de Mateo.
    [HttpGet("~/api/socios/{socioId:int}/prestamos")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PrestamoResponseDTO>>> ObtenerHistorialDeSocio(int socioId)
    {
        try
        {
            return Ok(await _prestamoService.ObtenerHistorialPorSocioAsync(socioId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
