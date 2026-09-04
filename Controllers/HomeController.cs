using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Definir datas (Hoje e Primeiro dia do Mês)
            var hoje = DateTime.Today;
            var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimDoMes = inicioDoMes.AddMonths(1).AddDays(-1);

            // 2. INDICADORES DOS CARDS
            ViewBag.TotalPacientes = await _context.Pacientes.CountAsync();
            ViewBag.SessoesHoje = await _context.Agendamentos.CountAsync(a => a.DataHora.Date == hoje);
            ViewBag.SessoesConcluidas = await _context.Agendamentos.CountAsync(a => a.DataHora.Date == hoje && a.Status.Contains("Realizado"));
            ViewBag.Pendencias = await _context.Agendamentos.CountAsync(a => a.DataHora.Date < hoje && !a.Status.Contains("Realizado") && !a.Status.Contains("Cancelado"));

            var cobrancasDoMes = await _context.Cobrancas
                .Where(c => c.Status == "Pago" && c.DataPagamento >= inicioDoMes && c.DataPagamento <= fimDoMes)
                .ToListAsync();

            ViewBag.ReceitaMes = cobrancasDoMes.Sum(c => c.Valor);

            // 3. PRÓXIMOS ATENDIMENTOS 
            ViewBag.ProximosAtendimentos = await _context.Agendamentos
                .Include(a => a.Paciente)
                .Where(a => a.DataHora.Date == hoje && a.DataHora >= DateTime.Now)
                .OrderBy(a => a.DataHora)
                .Take(5)
                .ToListAsync();

            // 4. DADOS PARA O GRÁFICO
            var sessoesMes = await _context.Agendamentos
                .Where(a => a.DataHora >= inicioDoMes && a.DataHora <= fimDoMes)
                .ToListAsync();

            ViewBag.TotalMes = sessoesMes.Count;
            ViewBag.Realizadas = sessoesMes.Count(a => a.Status.Contains("Realizado"));
            ViewBag.Agendadas = sessoesMes.Count(a => a.Status.Contains("Agendado"));
            ViewBag.Canceladas = sessoesMes.Count(a => a.Status.Contains("Cancelado"));
            ViewBag.Faltas = sessoesMes.Count(a => a.Status.Contains("Falta"));
            
            ViewBag.NomeMes = hoje.ToString("MMMM");

            return View();
        }
    }
}