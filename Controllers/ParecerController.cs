using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;
using NeuroSync.Services;

namespace NeuroSync.Controllers
{
    public class ParecerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CriptografiaService _criptografiaService;

        public ParecerController(AppDbContext context)
        {
            _context = context;
            _criptografiaService = new CriptografiaService();
        }

        // GET: Exibe o formulário de emissão para um prontuário específico
        [HttpGet]
        public IActionResult Criar(int prontuarioId)
        {
            var modelo = new ParecerTecnico
            {
                ProntuarioId = prontuarioId,
                DataEmissao = DateTime.Now
            };
            return View(modelo);
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Criar(ParecerTecnico parecer)
{
    ModelState.Remove("Prontuario");

    if (!ModelState.IsValid)
    {
        return View(parecer);
    }

    try
    {
        // Criptografa o conteúdo e define a data atual
        parecer.ConteudoCriptografado = _criptografiaService.Criptografar(parecer.ConteudoCriptografado);
        parecer.DataEmissao = DateTime.Now;

        // Salva diretamente na tabela de pareceres sem validações extras de prontuário
        _context.PareceresTecnicos.Add(parecer);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Pacientes", new { id = parecer.ProntuarioId });
    }
    catch (Exception ex)
    {
        Console.WriteLine("====== ERRO AO SALVAR PARECER: " + ex.Message + " =====px");
        if (ex.InnerException != null)
        {
            Console.WriteLine("====== INNER: " + ex.InnerException.Message + " ======");
        }
        ModelState.AddModelError(string.Empty, "Erro ao salvar o parecer: " + ex.Message);
    }

    return View(parecer);
}
        // GET: Visualizar o parecer descriptografado na tela
public async Task<IActionResult> Visualizar(int id)
{
    var parecer = await _context.PareceresTecnicos
        .FirstOrDefaultAsync(p => p.IdParecer == id);

    if (parecer == null) return NotFound();

    // Descriptografa para o profissional conseguir ler na tela
    ViewBag.ConteudoLegivel = _criptografiaService.Descriptografar(parecer.ConteudoCriptografado);

    return View(parecer);}
}
}