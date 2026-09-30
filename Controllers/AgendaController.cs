using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers;


[Authorize]
public class AgendaController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var agendamentos = await context.Agendamentos
            .Include(a => a.Paciente)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        return View(agendamentos);
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Agendamento agendamento, int semanasRepeticao = 1, decimal? valorSessao = null)
    {
        if (ModelState.IsValid)
        {
            var novasSessoes = new List<Agendamento>();

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

            await context.SaveChangesAsync();

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

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var agendamento = await context.Agendamentos.FindAsync(id.Value);
        if (agendamento == null) return NotFound();

        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", agendamento.PacienteId);
        return View(agendamento);
    }

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


    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var agendamento = await context.Agendamentos
            .Include(a => a.Paciente)
            .FirstOrDefaultAsync(a => a.IdAgendamento == id.Value);

        return agendamento == null ? NotFound() : View(agendamento);
    }

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