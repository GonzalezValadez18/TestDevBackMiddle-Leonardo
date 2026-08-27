using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDevBackMiddle.Data;
using TestDevBackMiddle.Models;

namespace TestDevBackMiddle.Controllers;

[ApiController]
[Route("logins")]
public class LoginsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoginsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /logins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Login>>> GetLogins()
    {
        var logins = await _context.Logins
            .OrderBy(l => l.Fecha)
            .ToListAsync();

        return Ok(logins);
    }

    // POST /logins
    [HttpPost]
    public async Task<ActionResult<Login>> CreateLogin(Login login)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.UserId == login.UserId);

        if (!userExists)
        {
            return BadRequest(
                $"El usuario con ID {login.UserId} no existe."
            );
        }

        // TipoMov únicamente puede ser:
        // 1 = Login
        // 0 = Logout
        if (login.TipoMov != 0 && login.TipoMov != 1)
        {
            return BadRequest(
                "TipoMov solo puede ser 1 para login o 0 para logout."
            );
        }

        // Validar que exista una fecha
        if (login.Fecha == default)
        {
            return BadRequest(
                "La fecha es obligatoria."
            );
        }

        var lastMovement = await _context.Logins
            .Where(l => l.UserId == login.UserId)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefaultAsync();

        if (lastMovement != null && login.Fecha <= lastMovement.Fecha)
        {
            return BadRequest(
                "La fecha debe ser mayor a la fecha del último movimiento"
            );
        }

        // No permitir dos logins consecutivos
        if (login.TipoMov == 1)
        {
            if (lastMovement != null && lastMovement.TipoMov == 1)
            {
                return BadRequest(
                    "El usuario ya tiene una sesión abierta."
                );
            }
        }

        // No permitir logout si no existe un login abierto
        if (login.TipoMov == 0)
        {
            if (lastMovement == null || lastMovement.TipoMov == 0)
            {
                return BadRequest(
                    "No se puede registrar un logout sin un login previo."
                );
            }
        }

        // Guardar movimiento
        _context.Logins.Add(login);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetLogins),
            new { id = login.Id },
            login
        );
    }

    // PUT /logins/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLogin(int id, Login login)
    {
        var existingLogin = await _context.Logins.FindAsync(id);

        if (existingLogin == null)
        {
            return NotFound($"El registro con el ID {id} no existe.");
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.UserId == login.UserId);

        if (!userExists)
        {
            return BadRequest(
                $"El usuario con el ID {login.UserId} no existe."
            );
        }

        if (login.TipoMov != 0 && login.TipoMov != 1)
        {
            return BadRequest(
                "TipoMov solo puede ser 1 para login o 0 para logout."
            );
        }

        if (login.Fecha == default)
        {
            return BadRequest("Debe incluir la fecha");
        }

        var previousMovement = await _context.Logins
            .Where(l =>
                l.UserId == login.UserId &&
                l.Id != id &&
                l.Fecha < login.Fecha)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefaultAsync();

        var nextMovement = await _context.Logins
            .Where(l =>
                l.UserId == login.UserId &&
                l.Id != id &&
                l.Fecha > login.Fecha)
            .OrderBy(l => l.Fecha)
            .FirstOrDefaultAsync();

        if (previousMovement != null &&
            previousMovement.TipoMov == login.TipoMov)
        {
            return BadRequest(
                "El movimiento tiene una secuencia invalida con el registro anterior."
            );
        }

        if (nextMovement != null &&
            nextMovement.TipoMov == login.TipoMov)
        {
            return BadRequest(
                "El movimiento tiene una secuencia invalida con el registro siguiente."
            );
        }

        if (login.TipoMov == 0 && previousMovement == null)
        {
            return BadRequest(
                "Necesita un logeo previo"
            );
        }

        existingLogin.UserId = login.UserId;
        existingLogin.Extension = login.Extension;
        existingLogin.TipoMov = login.TipoMov;
        existingLogin.Fecha = login.Fecha;

        await _context.SaveChangesAsync();

        return Ok(existingLogin);
    }
    // DELETE /logins/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLogin(int id)
    {
        var login = await _context.Logins.FindAsync(id);

        if (login == null)
        {
            return NotFound(
                $"El registro con ID {id} no existe."
            );
        }

        _context.Logins.Remove(login);

        await _context.SaveChangesAsync();

        return Ok(
            $"El registro con ID {id} fue eliminado correctamente."
        );
    }
}