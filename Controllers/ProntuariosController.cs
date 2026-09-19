using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador responsável pelo catálogo e acesso rápido aos Prontuários Eletrônicos dos Pacientes.
/// </summary>
[Authorize]
public class ProntuariosController(AppDbContext context) : Controller
{
    /// <summary>
    /// Lista todos os pacientes em ordem alfabética para seleção e abertura direta de prontuário.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var pacientes = await context.Pacientes
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(pacientes);
    }
}