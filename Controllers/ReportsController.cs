using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDevBackMiddle.Data;

namespace TestDevBackMiddle.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /reports/worked-hours/csv
    [HttpGet("worked-hours/csv")]
    public async Task<IActionResult> DownloadWorkedHoursCsv()
    {
        var users = await _context.Users.ToListAsync();
        var logins = await _context.Logins.ToListAsync();
        var areas = await _context.Areas.ToListAsync();

        var csv = new StringBuilder();

        csv.AppendLine("Login,NombreCompleto,Area,TotalHoras");

        foreach (var user in users)
        {
            var userLogins = logins
                .Where(l => l.UserId == user.UserId)
                .OrderBy(l => l.Fecha)
                .ToList();

            double totalHours = 0;

            for (int i = 0; i < userLogins.Count - 1; i++)
            {
                var current = userLogins[i];
                var next = userLogins[i + 1];

                if (current.TipoMov == 1 && next.TipoMov == 0)
                {
                    totalHours += (next.Fecha - current.Fecha).TotalHours;
                }
            }

            var area = areas
                .FirstOrDefault(a => a.IdArea == user.IDArea);

            var areaName = area?.AreaName ?? "Sin área";

            var fullName = string.Join(
                " ",
                new[]
                {
                    user.Nombres?.Trim(),
                    user.ApellidoPaterno?.Trim(),
                    user.ApellidoMaterno?.Trim()
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
            );

            csv.AppendLine(
                $"{EscapeCsv(user.Login)}," +
                $"{EscapeCsv(fullName)}," +
                $"{EscapeCsv(areaName)}," +
                $"{totalHours:F2}"
            );
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return File(
            bytes,
            "text/csv",
            "horas-trabajadas.csv"
        );
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n'))
        {
            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }

        return value;
    }
}