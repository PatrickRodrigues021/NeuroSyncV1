using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador principal do sistema: Dashboard Clínico, Resumo do Dia (Boas-Vindas) e Tratamento de Erros.
/// </summary>
[Authorize]
public class HomeController(AppDbContext context) : Controller
{
    // =========================================================================
    // 1. MÉTODOS AUXILIARES
    // =========================================================================

    /// <summary>
    /// Identifica o primeiro nome ou título da usuária conectada para saudações personalizadas.
    /// </summary>
    private async Task<string> ObterNomeExibicaoAsync()
    {
        string? nomeCompleto = null;

        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int idUsuario))
        {
            var usuario = await context.Usuarios.FindAsync(idUsuario);
            if (!string.IsNullOrWhiteSpace(usuario?.Nome)) nomeCompleto = usuario.Nome;
        }

        if (string.IsNullOrWhiteSpace(nomeCompleto) && User?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name) && User.Identity.Name != "admin")
        {
            nomeCompleto = User.Identity.Name;
        }

        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            var primeiroUsuario = await context.Usuarios.FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(primeiroUsuario?.Nome)) nomeCompleto = primeiroUsuario.Nome;
        }

        return ConfiguracoesController.ExtrairPrimeiroNome(nomeCompleto);
    }

    // =========================================================================
    // 2. DASHBOARD CLÍNICO PRINCIPAL
    // =========================================================================

    /// <summary>
    /// Carrega as métricas consolidadas do dia e do mês, gráfico de status e lista de atendimentos.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var agora = DateTime.Now;
        var hoje = DateTime.Today;
        var culture = new CultureInfo("pt-BR");

        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimDoMes = inicioDoMes.AddMonths(1).AddDays(-1);

        var saudacao = agora.Hour < 12 ? "Bom dia" : (agora.Hour < 18 ? "Boa tarde" : "Boa noite");
        string nomeExibicao = await ObterNomeExibicaoAsync();

        var model = new DashboardViewModel
        {
            Saudacao = saudacao,
            NomeUsuario = nomeExibicao,
            DataFormatada = culture.TextInfo.ToTitleCase(agora.ToString("dddd, d 'de' MMMM 'de' yyyy", culture)),
            NomeMes = culture.TextInfo.ToTitleCase(culture.DateTimeFormat.GetMonthName(hoje.Month))
        };

        // 1. Contadores gerais (Pacientes e Aniversariantes)
        model.TotalPacientesAtivos = await context.Pacientes.CountAsync();
        model.AniversariantesMes = await context.Pacientes.CountAsync(p => p.DataNascimento.Month == hoje.Month);

        // 2. Receita recebida no mês corrente
        var valoresRecebidos = await context.Cobrancas
            .Where(c => c.Status == "Pago" && c.DataPagamento >= inicioDoMes && c.DataPagamento <= fimDoMes)
            .Select(c => c.Valor)
            .ToListAsync();
        model.ReceitaMes = valoresRecebidos.Sum();

        // 3. Atendimentos passados pendentes de registro clínico
        model.PendenciasHoje = await context.Agendamentos
            .CountAsync(a => a.DataHora.Date < hoje && !a.Status.Contains("Realizado") && !a.Status.Contains("Cancelado"));

        // 4. Status consolidado das sessões do mês
        var sessoesMes = await context.Agendamentos
            .Where(a => a.DataHora >= inicioDoMes && a.DataHora <= fimDoMes)
            .ToListAsync();

        if (sessoesMes.Count > 0)
        {
            model.TotalSessoesMes = sessoesMes.Count;
            model.RealizadasMes = sessoesMes.Count(a => a.Status.Contains("Realizado"));
            model.AgendadasMes = sessoesMes.Count(a => a.Status.Contains("Agendado") || a.Status.Contains("Confirmado"));
            model.CanceladasMes = sessoesMes.Count(a => a.Status.Contains("Cancelado"));
            model.FaltasMes = sessoesMes.Count(a => a.Status.Contains("Falta"));
        }
        else
        {
            // Dados de demonstração harmoniosos caso o banco esteja no primeiro dia do mês
            model.TotalSessoesMes = 13;
            model.RealizadasMes = 2;
            model.AgendadasMes = 10;
            model.CanceladasMes = 1;
            model.FaltasMes = 0;
        }

        model.SessoesRealizadasMes = model.RealizadasMes;

        // 5. Atendimentos previstos para hoje
        var agendamentosHoje = await context.Agendamentos
            .Include(a => a.Paciente)
            .Where(a => a.DataHora.Date == hoje)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        if (agendamentosHoje.Count > 0)
        {
            model.AtendimentosHoje = agendamentosHoje.Count;
            model.ConcluidosHoje = agendamentosHoje.Count(a => a.Status != null && a.Status.Contains("Realizado"));
            model.ConfirmadosHoje = agendamentosHoje.Count(a => a.Status != null && (a.Status.Contains("Confirmado") || a.Status.Contains("Realizado") || a.Status.Contains("Agendado")));
            model.AguardandoConfirmacaoHoje = Math.Max(0, model.AtendimentosHoje - model.ConfirmadosHoje);

            model.ConfirmadosPercentual = (int)Math.Round((double)model.ConfirmadosHoje / model.AtendimentosHoje * 100);
            model.AguardandoPercentual = 100 - model.ConfirmadosPercentual;

            int i = 0;
            foreach (var ag in agendamentosHoje.Take(5))
            {
                i++;
                model.ProximosAtendimentos.Add(new DashboardAtendimentoItem
                {
                    Horario = ag.DataHora.ToString("HH:mm"),
                    PacienteNome = ag.Paciente?.Nome ?? "Paciente",
                    Terapia = !string.IsNullOrEmpty(ag.TipoSessao) ? ag.TipoSessao : "Intervenção Cognitiva",
                    Sala = $"Consultório 0{((i % 2) + 1)}",
                    Status = !string.IsNullOrEmpty(ag.Status) ? ag.Status : "Confirmado",
                    AvatarGenero = (i % 2 == 0) ? "girl" : "boy",
                    PacienteId = ag.PacienteId,
                    AgendamentoId = ag.IdAgendamento
                });
            }
        }
        else
        {
            // Carga demonstrativa de rotina para apresentação visual inicial
            model.AtendimentosHoje = 8;
            model.ConcluidosHoje = 2;
            model.ConfirmadosHoje = 6;
            model.ConfirmadosPercentual = 75;
            model.AguardandoConfirmacaoHoje = 2;
            model.AguardandoPercentual = 25;
            if (model.PendenciasHoje == 0) model.PendenciasHoje = 5;

            var primeiroPaciente = await context.Pacientes.FirstOrDefaultAsync();
            int defaultPacId = primeiroPaciente?.IdPaciente ?? 1;

            model.ProximosAtendimentos = [
                new() { Horario = "09:00", PacienteNome = "João Silva", Terapia = "Intervenção Cognitiva", Sala = "Consultório 01", Status = "Confirmado", AvatarGenero = "boy", PacienteId = defaultPacId, AgendamentoId = 1 },
                new() { Horario = "10:30", PacienteNome = "Maria Oliveira", Terapia = "Avaliação Neuropsicopedagógica", Sala = "Consultório 01", Status = "Aguardando confirmação", AvatarGenero = "girl", PacienteId = defaultPacId, AgendamentoId = 2 },
                new() { Horario = "14:00", PacienteNome = "Pedro Santos", Terapia = "Intervenção Cognitiva", Sala = "Consultório 01", Status = "Em atendimento", AvatarGenero = "boy", PacienteId = defaultPacId, AgendamentoId = 3 },
                new() { Horario = "15:30", PacienteNome = "Ana Beatriz M.", Terapia = "Orientação Escolar", Sala = "Consultório 01", Status = "Confirmado", AvatarGenero = "girl", PacienteId = defaultPacId, AgendamentoId = 4 },
                new() { Horario = "16:45", PacienteNome = "Lucas Ferreira", Terapia = "Devolutiva com Pais", Sala = "Consultório 01", Status = "Confirmado", AvatarGenero = "boy", PacienteId = defaultPacId, AgendamentoId = 5 }
            ];
        }

        // 6. Ritmo Semanal (Distribuição Seg a Sex)
        int diff = (7 + (hoje.DayOfWeek - DayOfWeek.Monday)) % 7;
        var inicioSemana = hoje.AddDays(-diff).Date;
        var fimSemana = inicioSemana.AddDays(6).Date;

        var sessoesSemana = await context.Agendamentos
            .Where(a => a.DataHora.Date >= inicioSemana && a.DataHora.Date <= fimSemana)
            .ToListAsync();

        if (sessoesSemana.Count > 0)
        {
            model.SemanaSeg = sessoesSemana.Count(a => a.DataHora.DayOfWeek == DayOfWeek.Monday);
            model.SemanaTer = sessoesSemana.Count(a => a.DataHora.DayOfWeek == DayOfWeek.Tuesday);
            model.SemanaQua = sessoesSemana.Count(a => a.DataHora.DayOfWeek == DayOfWeek.Wednesday);
            model.SemanaQui = sessoesSemana.Count(a => a.DataHora.DayOfWeek == DayOfWeek.Thursday);
            model.SemanaSex = sessoesSemana.Count(a => a.DataHora.DayOfWeek == DayOfWeek.Friday);
        }
        else
        {
            model.SemanaSeg = 4; model.SemanaTer = 5; model.SemanaQua = 6; model.SemanaQui = 4; model.SemanaSex = 3;
        }

        return View(model);
    }

    // =========================================================================
    // 3. TELA DE BOAS-VINDAS / RESUMO EXECUTIVO DO DIA
    // =========================================================================

    /// <summary>
    /// Exibe o panorama executivo diário: atendimentos de hoje, aniversariantes do mês e atalhos ágeis.
    /// </summary>
    public async Task<IActionResult> BoasVindas()
    {
        var agora = DateTime.Now;
        var hoje = DateTime.Today;
        var culture = new CultureInfo("pt-BR");

        var model = new ResumoDoDiaViewModel
        {
            Saudacao = agora.Hour < 12 ? "Bom dia" : (agora.Hour < 18 ? "Boa tarde" : "Boa noite"),
            NomeProfissional = await ObterNomeExibicaoAsync(),
            DataFormatada = culture.TextInfo.ToTitleCase(agora.ToString("dddd, d 'de' MMMM 'de' yyyy", culture)),
            HoraFormatada = agora.ToString("HH:mm")
        };

        var agendamentosHoje = await context.Agendamentos
            .Include(a => a.Paciente)
            .Where(a => a.DataHora.Date == hoje)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        var aniversariantesDoMes = await context.Pacientes
            .Where(p => p.DataNascimento.Month == hoje.Month)
            .OrderBy(p => p.DataNascimento.Day)
            .ToListAsync();

        if (agendamentosHoje.Count > 0)
        {
            model.TotalAtendimentosHoje = agendamentosHoje.Count;
            model.ConfirmadosCount = agendamentosHoje.Count(a => a.Status != null && (a.Status.Contains("Confirmado") || a.Status.Contains("Agendado") || a.Status.Contains("Realizado")));
            model.AguardandoCount = Math.Max(0, model.TotalAtendimentosHoje - model.ConfirmadosCount);
            model.ConfirmadosPercentual = (int)Math.Round((double)model.ConfirmadosCount / model.TotalAtendimentosHoje * 100);
            model.AguardandoPercentual = 100 - model.ConfirmadosPercentual;

            int idx = 0;
            foreach (var ag in agendamentosHoje.Take(3))
            {
                idx++;
                var idadeStr = ag.Paciente?.Idade > 0 ? $"{ag.Paciente.Idade} anos • " : "";
                model.ProximosAtendimentos.Add(new AtendimentoResumoItem
                {
                    Horario = ag.DataHora.ToString("HH:mm"),
                    NomePaciente = ag.Paciente?.Nome ?? "Paciente",
                    Detalhes = $"{idadeStr}{(!string.IsNullOrEmpty(ag.TipoSessao) ? ag.TipoSessao : "Sessão")}",
                    Status = !string.IsNullOrEmpty(ag.Status) ? ag.Status : "Confirmado",
                    GeneroOuAvatar = (idx % 2 == 0) ? "girl" : "boy"
                });
            }

            model.MaisAtendimentosHojeCount = Math.Max(0, agendamentosHoje.Count - 3);
        }
        else
        {
            model.TotalAtendimentosHoje = 6;
            model.ConfirmadosCount = 5;
            model.ConfirmadosPercentual = 83;
            model.AguardandoCount = 1;
            model.AguardandoPercentual = 17;
            model.MaisAtendimentosHojeCount = 3;

            model.ProximosAtendimentos = [
                new() { Horario = "09:00", NomePaciente = "João Pedro S.", Detalhes = "10 anos • Avaliação", Status = "Confirmado", GeneroOuAvatar = "boy" },
                new() { Horario = "10:30", NomePaciente = "Maria Clara L.", Detalhes = "8 anos • Intervenção", Status = "Confirmado", GeneroOuAvatar = "girl" },
                new() { Horario = "14:00", NomePaciente = "Lucas R.", Detalhes = "11 anos • Intervenção", Status = "Confirmado", GeneroOuAvatar = "boy" }
            ];
        }

        if (aniversariantesDoMes.Count > 0)
        {
            model.AniversariantesMesCount = aniversariantesDoMes.Count;
            int idx = 0;
            foreach (var p in aniversariantesDoMes.Take(4))
            {
                idx++;
                var idadeStr = p.Idade > 0 ? $"{p.Idade} anos • " : "";
                model.Aniversariantes.Add(new AniversarianteResumoItem
                {
                    NomePaciente = p.Nome,
                    Detalhes = $"{idadeStr}{p.DataNascimento.ToString("d 'de' MMMM", culture)}",
                    GeneroOuAvatar = (idx % 2 == 0) ? "girl" : "boy"
                });
            }
        }
        else
        {
            model.AniversariantesMesCount = 2;
            var mesNome = culture.DateTimeFormat.GetMonthName(hoje.Month);
            model.Aniversariantes = [
                new() { NomePaciente = "João Pedro S.", Detalhes = $"10 anos • 12 de {mesNome}", GeneroOuAvatar = "boy" },
                new() { NomePaciente = "Maria Clara L.", Detalhes = $"8 anos • 27 de {mesNome}", GeneroOuAvatar = "girl" }
            ];
        }

        return View(model);
    }

    // =========================================================================
    // 4. TRATAMENTO DE ERROS DA APLICAÇÃO
    // =========================================================================

    /// <summary>
    /// Exibe a página amigável de erro não tratado com rastreamento da requisição (RequestId).
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}