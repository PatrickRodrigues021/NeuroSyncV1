using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;

namespace NeuroSync.Controllers;

[Authorize]
public class ProntuariosController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var pacientes = await context.Pacientes
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(pacientes);
    }
}