using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador responsável pela gestão da agenda clínica e agendamento de sessões.
/// Suporta recorrência automática (semanas) e geração integrada de cobrança por sessão.
/// </summary>
[Authorize]
public class AgendaController(AppDbContext context) : Controller
{
    // =========================================================================
    // 1. LISTAGEM DA AGENDA
    // =========================================================================

    /// <summary>
    /// Exibe a lista completa de atendimentos agendados em ordem cronológica.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var agendamentos = await context.Agendamentos
            .Include(a => a.Paciente)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        return View(agendamentos);
    }

    // =========================================================================
    // 2. NOVO AGENDAMENTO
    // =========================================================================

    /// <summary>
    /// Abre o formulário para cadastro de um novo agendamento.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(int? pacienteId, string? data)
    {
        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", pacienteId);
        
        var agora = DateTime.Now;
        DateTime dataSugerida;

        if (!string.IsNullOrWhiteSpace(data) && DateTime.TryParse(data, out var dataInformada))
        {
            dataSugerida = dataInformada;
        }
        else if (agora.Hour >= 8 && agora.Hour < 18)
        {
            dataSugerida = DateTime.Today.AddHours(agora.Hour + 1);
        }
        else
        {
            var dia = agora.Hour >= 18 ? DateTime.Today.AddDays(1) : DateTime.Today;
            dataSugerida = dia.AddHours(9);
        }

        var agendamento = new Agendamento
        {
            DataHora = dataSugerida
        };

        if (pacienteId.HasValue) agendamento.PacienteId = pacienteId.Value;
        return View(agendamento);
    }

    /// <summary>
    /// Salva o agendamento com suporte à repetição semanal (1, 4, 12 ou 24 semanas) e geração de cobrança.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Agendamento agendamento, int semanasRepeticao = 1, decimal? valorSessao = null)
    {
        if (ModelState.IsValid)
        {
            var novasSessoes = new List<Agendamento>();

            // Cria os registros das sessões recorrentes de acordo com o intervalo selecionado
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

                context.Agendamentos.Add(novaSessao);
                novasSessoes.Add(novaSessao);
            }

            // Persiste as sessões para obter os Ids gerados
            await context.SaveChangesAsync();

            // Gera cobranças pendentes automáticas caso um valor por sessão tenha sido informado
            if (valorSessao is > 0)
            {
                foreach (var sessao in novasSessoes)
                {
                    context.Cobrancas.Add(new Cobranca
                    {
                        PacienteId = sessao.PacienteId,
                        AgendamentoId = sessao.IdAgendamento,
                        Valor = valorSessao.Value,
                        DataVencimento = sessao.DataHora.Date,
                        Status = "Pendente",
                        Descricao = $"Sessão de {sessao.TipoSessao} - {sessao.DataHora:dd/MM/yyyy}"
                    });
                }
                await context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", agendamento.PacienteId);
        return View(agendamento);
    }

    // =========================================================================
    // 3. EDIÇÃO DE AGENDAMENTO
    // =========================================================================

    /// <summary>
    /// Abre o formulário de edição de um atendimento existente.
    /// </summary>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var agendamento = await context.Agendamentos.FindAsync(id.Value);
        if (agendamento == null) return NotFound();

        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", agendamento.PacienteId);
        return View(agendamento);
    }

    /// <summary>
    /// Salva as alterações efetuadas em um atendimento.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Agendamento agendamento)
    {
        if (id != agendamento.IdAgendamento) return NotFound();

        if (ModelState.IsValid)
        {
            context.Update(agendamento);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", agendamento.PacienteId);
        return View(agendamento);
    }

    // =========================================================================
    // 4. EXCLUSÃO DE AGENDAMENTO
    // =========================================================================

    /// <summary>
    /// Abre a tela de confirmação de exclusão do agendamento.
    /// </summary>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var agendamento = await context.Agendamentos
            .Include(a => a.Paciente)
            .FirstOrDefaultAsync(a => a.IdAgendamento == id.Value);

        return agendamento == null ? NotFound() : View(agendamento);
    }

    /// <summary>
    /// Executa a exclusão definitiva do agendamento selecionado.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var agendamento = await context.Agendamentos.FindAsync(id);
        if (agendamento != null)
        {
            context.Agendamentos.Remove(agendamento);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 5. TRANSIÇÃO CLÍNICA: INICIAR ATENDIMENTO
    // =========================================================================

    /// <summary>
    /// Marca o agendamento como "Em atendimento" e redireciona para a aba de evolução do paciente.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> IniciarAtendimento(int? id, int? pacienteId)
    {
        int idPacienteDestino = pacienteId ?? 0;

        if (id is > 0)
        {
            var agendamento = await context.Agendamentos.FindAsync(id.Value);
            if (agendamento != null)
            {
                agendamento.Status = "Em atendimento";
                await context.SaveChangesAsync();
                idPacienteDestino = agendamento.PacienteId;
            }
        }

        return idPacienteDestino > 0
            ? RedirectToAction("Details", "Pacientes", new { id = idPacienteDestino, aba = "evolucao" })
            : RedirectToAction(nameof(Index));
    }
}