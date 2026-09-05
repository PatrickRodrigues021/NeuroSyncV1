using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers
{
    [Authorize]
    public class TerapiasController : Controller
    {
        private readonly AppDbContext _context;

        // Lista gerenciada de terapias para catálogo ágil da clínica
        private static readonly List<Terapia> _terapiasCadastradas = new()
        {
            new Terapia
            {
                Id = 1,
                Nome = "Fonoaudiologia",
                Descricao = "Avaliação e tratamento de alterações na comunicação oral e escrita.",
                Icone = "bi-chat-quote-fill",
                CorHex = "#2563EB",
                CorFundoHex = "#EFF6FF",
                PacientesAtivosCount = 56,
                ProfissionaisCount = 3,
                SessoesMesCount = 142
            },
            new Terapia
            {
                Id = 2,
                Nome = "Psicologia",
                Descricao = "Acompanhamento psicológico e desenvolvimento emocional.",
                Icone = "bi-balloon-heart-fill",
                CorHex = "#8B5CF6",
                CorFundoHex = "#F5F3FF",
                PacientesAtivosCount = 42,
                ProfissionaisCount = 4,
                SessoesMesCount = 118
            },
            new Terapia
            {
                Id = 3,
                Nome = "Terapia Ocupacional",
                Descricao = "Desenvolvimento de habilidades funcionais e autonomia.",
                Icone = "bi-hand-index-thumb-fill",
                CorHex = "#0D9488",
                CorFundoHex = "#F0FDFA",
                PacientesAtivosCount = 28,
                ProfissionaisCount = 2,
                SessoesMesCount = 86
            },
            new Terapia
            {
                Id = 4,
                Nome = "Psicopedagogia",
                Descricao = "Intervenção em dificuldades de aprendizagem.",
                Icone = "bi-puzzle-fill",
                CorHex = "#E11D48",
                CorFundoHex = "#FFF1F2",
                PacientesAtivosCount = 18,
                ProfissionaisCount = 2,
                SessoesMesCount = 45
            },
            new Terapia
            {
                Id = 5,
                Nome = "Fisioterapia",
                Descricao = "Tratamentos para desenvolvimento motor e funcional.",
                Icone = "bi-person-walking",
                CorHex = "#0284C7",
                CorFundoHex = "#F0F9FF",
                PacientesAtivosCount = 16,
                ProfissionaisCount = 2,
                SessoesMesCount = 38
            }
        };

        public TerapiasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Terapias
        public IActionResult Index()
        {
            return View(_terapiasCadastradas);
        }

        // POST: /Terapias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Terapia model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                TempData["Erro"] = "O nome da terapia é obrigatório.";
                return RedirectToAction(nameof(Index));
            }

            model.Id = _terapiasCadastradas.Any() ? _terapiasCadastradas.Max(t => t.Id) + 1 : 1;
            
            // Atribuição de cores padrão se não vier preenchido
            if (string.IsNullOrWhiteSpace(model.CorHex)) model.CorHex = "#315BEF";
            if (string.IsNullOrWhiteSpace(model.CorFundoHex)) model.CorFundoHex = "#EFF6FF";
            if (string.IsNullOrWhiteSpace(model.Icone)) model.Icone = "bi-puzzle-fill";

            _terapiasCadastradas.Add(model);
            TempData["Sucesso"] = $"Terapia {model.Nome} cadastrada com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Terapias/Detalhes/1
        public async Task<IActionResult> Detalhes(int id)
        {
            var terapia = _terapiasCadastradas.FirstOrDefault(t => t.Id == id);
            if (terapia == null)
            {
                return NotFound();
            }

            // Busca profissionais cadastrados
            var profissionais = await _context.Profissionais
                .Where(p => p.Especialidade.Contains(terapia.Nome) || p.Especialidade.Contains(terapia.Nome.Substring(0, Math.Min(4, terapia.Nome.Length))))
                .ToListAsync();

            if (!profissionais.Any())
            {
                profissionais = await _context.Profissionais.Take(3).ToListAsync();
            }

            // Busca pacientes cadastrados
            var pacientes = await _context.Pacientes
                .OrderByDescending(p => p.DataCadastro)
                .Take(10)
                .ToListAsync();

            // Busca agendamentos recentes
            var agendamentos = await _context.Agendamentos
                .Include(a => a.Paciente)
                .OrderByDescending(a => a.DataHora)
                .Take(5)
                .ToListAsync();

            var viewModel = new TerapiaDetalhesViewModel
            {
                Terapia = terapia,
                Profissionais = profissionais,
                Pacientes = pacientes,
                ProximosAgendamentos = agendamentos
            };

            return View(viewModel);
        }
    }
}

