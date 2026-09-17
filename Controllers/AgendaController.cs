using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NeuroSync.Data;
using NeuroSync.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;


//teste
namespace NeuroSync.Controllers
{
    [Authorize]
    public class AgendaController : Controller
    {
        private readonly AppDbContext _context;

        public AgendaController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TELA PRINCIPAL (Lista de horários)
        public IActionResult Index()
        {
            var agendamentos = _context.Agendamentos
                                       .Include(a => a.Paciente)
                                       .OrderBy(a => a.DataHora)
                                       .ToList();

            return View(agendamentos);
        }

        // 2. GET: Abre a tela de Novo Agendamento
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome");
            return View();
        }

        // 3. POST: Salva a sessão (com Repetição e Cobrança)
        [HttpPost]
        public IActionResult Create(Agendamento agendamento, int semanasRepeticao = 1, decimal? valorSessao = null)
        {
            if (ModelState.IsValid)
            {
                var novasSessoes = new List<Agendamento>();

                // O laço de repetição: vai rodar 1, 4, 12 ou 24 vezes dependendo da escolha
                for (int i = 0; i < semanasRepeticao; i++)
                {
                    var novaSessao = new Agendamento
                    {
                        PacienteId = agendamento.PacienteId,
                        DataHora = agendamento.DataHora.AddDays(7 * i),
                        TipoSessao = agendamento.TipoSessao,
                        Observacoes = agendamento.Observacoes,
                        Status = "Agendado" 
                    };
                    
                    _context.Agendamentos.Add(novaSessao);
                    novasSessoes.Add(novaSessao);
                }
                
                // Salva todas as sessões primeiro para o banco gerar o Id de cada uma
                _context.SaveChanges();

                // --- GERAÇÃO AUTOMÁTICA DE COBRANÇA ---
                if (valorSessao.HasValue && valorSessao.Value > 0)
                {
                    foreach (var sessao in novasSessoes)
                    {
                        var novaCobranca = new Cobranca
                        {
                            PacienteId = sessao.PacienteId,
                            AgendamentoId = sessao.IdAgendamento,
                            Valor = valorSessao.Value,
                            DataVencimento = sessao.DataHora.Date,
                            Status = "Pendente",
                            Descricao = $"Sessão de {sessao.TipoSessao} - {sessao.DataHora:dd/MM/yyyy}"
                        };

                        _context.Cobrancas.Add(novaCobranca);
                    }
                    _context.SaveChanges();
                }
                
                return RedirectToAction("Index"); 
            }
            
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome", agendamento.PacienteId);
            return View(agendamento);
        }

        // 4. GET: Abre a tela de edição
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var agendamento = _context.Agendamentos.Find(id);
            if (agendamento == null) return NotFound();

            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome", agendamento.PacienteId);
            return View(agendamento);
        }

        // 5. POST: Salva as alterações da sessão
        [HttpPost]
        public IActionResult Edit(int id, Agendamento agendamento)
        {
            if (id != agendamento.IdAgendamento) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(agendamento);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome", agendamento.PacienteId);
            return View(agendamento);
        }

        // 6. GET: Abre a tela de confirmação de exclusão
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var agendamento = _context.Agendamentos
                                      .Include(a => a.Paciente)
                                      .FirstOrDefault(a => a.IdAgendamento == id);
            
            if (agendamento == null) return NotFound();

            return View(agendamento);
        }

        // 7. POST: Apaga a sessão
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var agendamento = _context.Agendamentos.Find(id);
            
            if (agendamento != null)
            {
                _context.Agendamentos.Remove(agendamento);
                _context.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        // 8. GET: Inicia o atendimento do paciente e abre o prontuário
        [HttpGet]
        public async Task<IActionResult> IniciarAtendimento(int? id, int? pacienteId)
        {
            int idPacienteDestino = pacienteId ?? 0;

            if (id.HasValue && id.Value > 0)
            {
                var agendamento = await _context.Agendamentos.FindAsync(id.Value);
                if (agendamento != null)
                {
                    agendamento.Status = "Em atendimento";
                    await _context.SaveChangesAsync();
                    idPacienteDestino = agendamento.PacienteId;
                }
            }

            if (idPacienteDestino > 0)
            {
                return RedirectToAction("Details", "Pacientes", new { id = idPacienteDestino, aba = "evolucao" });
            }

            return RedirectToAction("Index");
        }
    }
}