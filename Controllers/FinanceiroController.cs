using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador responsável pelo módulo Financeiro: Faturamento de Pacientes (Receitas),
/// Despesas Operacionais da Clínica (Contas), Comparativos em Gráfico e Exportações.
/// </summary>
[Authorize]
public class FinanceiroController(AppDbContext context, IWebHostEnvironment hostEnvironment) : Controller
{
    // =========================================================================
    // 1. MÉTODOS DE FILTRAGEM
    // =========================================================================

    /// <summary>
    /// Consulta cobranças filtradas por paciente, status e competência (Ano-Mês).
    /// </summary>
    private List<Cobranca> ObterCobrancasFiltradas(int? pacienteId, string? status, string? competencia)
    {
        var query = context.Cobrancas
            .Include(c => c.Paciente)
            .Include(c => c.Agendamento)
            .AsQueryable();

        query = query.Where(c => c.AgendamentoId == null || (c.Agendamento != null && c.Agendamento.Status == "Realizado"));

        if (pacienteId is > 0)
            query = query.Where(c => c.PacienteId == pacienteId.Value);

        if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(competencia) &&
            DateTime.TryParseExact(competencia, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var competenciaData))
        {
            query = query.Where(c => c.DataVencimento.Year == competenciaData.Year && c.DataVencimento.Month == competenciaData.Month);
        }

        return query.OrderBy(c => c.DataVencimento).ToList();
    }

    /// <summary>
    /// Consulta despesas filtradas por categoria, status (Pagas/Pendentes) e competência.
    /// </summary>
    private List<Despesa> ObterDespesasFiltradas(string? categoria, string? status, string? competencia)
    {
        var query = context.Despesas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todas")
            query = query.Where(d => d.Categoria == categoria);

        if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
            query = query.Where(d => d.Status == status);

        if (!string.IsNullOrWhiteSpace(competencia) &&
            DateTime.TryParseExact(competencia, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var compData))
        {
            query = query.Where(d => d.DataVencimento.Year == compData.Year && d.DataVencimento.Month == compData.Month);
        }

        return query.OrderBy(d => d.DataVencimento).ToList();
    }

    // =========================================================================
    // 2. TELA PRINCIPAL (RESUMO, RECEITAS E DESPESAS)
    // =========================================================================

    /// <summary>
    /// Carrega as três visões financeiras: KPIs consolidados de resultado líquido, cobranças e despesas operacionais.
    /// </summary>
    public async Task<IActionResult> Index(
        string? aba,
        int? pacienteId,
        string? status,
        string? competencia,
        string? despesaCategoria,
        string? despesaStatus,
        string? despesaCompetencia)
    {
        var cobrancas = ObterCobrancasFiltradas(pacienteId, status, competencia);
        var despesas = ObterDespesasFiltradas(despesaCategoria, despesaStatus, despesaCompetencia);

        var hoje = DateTime.Today;
        var inicioMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMesAtual = inicioMesAtual.AddMonths(1).AddDays(-1);

        // Cobranças e despesas do mês corrente para o Resumo
        var cobrancasMes = await context.Cobrancas
            .Where(c => c.DataVencimento >= inicioMesAtual && c.DataVencimento <= fimMesAtual)
            .ToListAsync();

        var despesasMes = await context.Despesas
            .Where(d => d.DataVencimento >= inicioMesAtual && d.DataVencimento <= fimMesAtual)
            .ToListAsync();

        decimal receitaMes = cobrancasMes.Where(c => c.Status == "Pago").Sum(c => c.Valor);
        if (receitaMes == 0) receitaMes = cobrancasMes.Sum(c => c.Valor);

        decimal despesaMesTotal = despesasMes.Sum(d => d.Valor);
        decimal resultadoLiquido = receitaMes - despesaMesTotal;

        var todasCobrancasAbertas = await context.Cobrancas
            .Include(c => c.Paciente)
            .Where(c => c.Status == "Pendente" || c.Status == "Atrasado")
            .ToListAsync();
        decimal totalContasReceber = todasCobrancasAbertas.Sum(c => c.Valor);

        // Histórico comparativo Receita x Despesa dos últimos 5 meses
        string[] mesesNomes = ["", "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];
        List<string> mesesLabels = [];
        List<decimal> receitasMeses = [];
        List<decimal> despesasMeses = [];

        for (int i = 4; i >= 0; i--)
        {
            var m = hoje.AddMonths(-i);
            mesesLabels.Add(mesesNomes[m.Month]);

            var ini = new DateTime(m.Year, m.Month, 1);
            var fim = ini.AddMonths(1).AddDays(-1);

            var recValores = await context.Cobrancas
                .Where(c => c.DataVencimento >= ini && c.DataVencimento <= fim && (c.Status == "Pago" || c.Status == "Pendente"))
                .Select(c => c.Valor)
                .ToListAsync();
            var rec = recValores.Sum();

            var despValores = await context.Despesas
                .Where(d => d.DataVencimento >= ini && d.DataVencimento <= fim)
                .Select(d => d.Valor)
                .ToListAsync();
            var desp = despValores.Sum();

            receitasMeses.Add(rec);
            despesasMeses.Add(desp);
        }

        // Distribuição de despesas por categoria para o Donut Chart
        var categoriasAgrupadas = despesasMes
            .GroupBy(d => d.Categoria)
            .OrderByDescending(g => g.Sum(x => x.Valor))
            .ToList();

        var categoriasLabels = categoriasAgrupadas.Select(g => g.Key).ToList();
        var categoriasValores = categoriasAgrupadas.Select(g => g.Sum(x => x.Valor)).ToList();

        // Movimentações recentes
        var ultimasReceitas = await context.Cobrancas
            .Include(c => c.Paciente)
            .OrderByDescending(c => c.DataPagamento ?? c.DataVencimento)
            .Take(5)
            .ToListAsync();

        var proximasDespesas = await context.Despesas
            .Where(d => d.Status == "Pendente")
            .OrderBy(d => d.DataVencimento)
            .Take(5)
            .ToListAsync();

        // KPIs específicos
        decimal despesasPagas = despesas.Where(d => d.Status == "Pago").Sum(d => d.Valor);
        decimal despesasAPagar = despesas.Where(d => d.Status == "Pendente").Sum(d => d.Valor);
        string maiorCategoria = categoriasLabels.FirstOrDefault() ?? "Geral";

        decimal totalFaturado = cobrancas.Sum(c => c.Valor);
        decimal totalRecebido = cobrancas.Where(c => c.Status == "Pago").Sum(c => c.Valor);
        decimal totalAReceber = cobrancas.Where(c => c.Status == "Pendente" || c.Status == "Atrasado").Sum(c => c.Valor);
        decimal totalAtrasado = cobrancas.Where(c => c.Status == "Atrasado").Sum(c => c.Valor);

        var viewModel = new FinanceiroViewModel
        {
            AbaAtiva = string.IsNullOrWhiteSpace(aba) ? "Resumo" : aba,
            Cobrancas = cobrancas,
            TotalRecebido = totalRecebido,
            TotalAReceber = totalAReceber,
            TotalEmAberto = totalAReceber,
            PercentualRecebido = totalFaturado > 0 ? Math.Round((double)totalRecebido / (double)totalFaturado * 100, 1) : 0,
            PercentualAReceber = totalFaturado > 0 ? Math.Round((double)totalAReceber / (double)totalFaturado * 100, 1) : 0,
            TaxaInadimplencia = totalFaturado > 0 ? Math.Round((double)totalAtrasado / (double)totalFaturado * 100, 1) : 0,
            PacienteId = pacienteId,
            StatusSelecionado = status ?? "Todos",
            CompetenciaSelecionada = competencia ?? "",
            ReceitaMes = receitaMes,
            TotalDespesasMes = despesaMesTotal,
            DespesasPagasMes = despesasPagas,
            DespesasPendentesMes = despesasAPagar,
            SaldoLiquidoMes = resultadoLiquido,
            MesesLabels = mesesLabels,
            ReceitasMensais = receitasMeses,
            DespesasMensais = despesasMeses,
            CategoriasLabels = categoriasLabels,
            CategoriasValores = categoriasValores,
            UltimasReceitas = ultimasReceitas,
            ProximasDespesas = proximasDespesas,
            Despesas = despesas,
            MaiorCategoriaDespesa = maiorCategoria,
            CategoriaDespesaSelecionada = despesaCategoria ?? "Todas",
            StatusDespesaSelecionado = despesaStatus ?? "Todos"
        };

        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", pacienteId);
        ViewBag.StatusList = new SelectList(new[] { "Todos", "Pendente", "Pago", "Atrasado", "Cancelado" }, status);
        ViewBag.CategoriasDespesa = new SelectList(new[] { "Todas", "Energia Elétrica", "Água", "Telefonia", "Internet", "Material de Escritório", "Sistemas", "Aluguel", "Limpeza", "Outros" }, despesaCategoria);

        return View(viewModel);
    }

    // =========================================================================
    // 3. GESTÃO DE DESPESAS DA CLÍNICA
    // =========================================================================

    /// <summary>
    /// Cadastra uma nova despesa da clínica via modal.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarDespesa(Despesa despesa)
    {
        if (ModelState.IsValid)
        {
            if (despesa.Status == "Pago" && !despesa.DataPagamento.HasValue)
                despesa.DataPagamento = DateTime.Today;

            context.Despesas.Add(despesa);
            await context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Despesa \"{despesa.Descricao}\" cadastrada com sucesso!";
            return RedirectToAction(nameof(Index), new { aba = "Despesas" });
        }

        TempData["MensagemErro"] = "Preencha todos os campos obrigatórios da despesa.";
        return RedirectToAction(nameof(Index), new { aba = "Despesas" });
    }

    /// <summary>
    /// Registra o pagamento efetuado de uma despesa da clínica.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BaixarDespesa(int id, DateTime? dataPagamento)
    {
        var despesa = await context.Despesas.FindAsync(id);
        if (despesa != null)
        {
            despesa.Status = "Pago";
            despesa.DataPagamento = dataPagamento ?? DateTime.Today;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = $"Pagamento da despesa \"{despesa.Descricao}\" registrado com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Despesas" });
    }

    /// <summary>
    /// Reabre uma despesa previamente marcada como paga, retornando-a para o status Pendente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReabrirDespesa(int id)
    {
        var despesa = await context.Despesas.FindAsync(id);
        if (despesa != null)
        {
            despesa.Status = "Pendente";
            despesa.DataPagamento = null;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = $"Despesa \"{despesa.Descricao}\" reaberta com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Despesas" });
    }

    /// <summary>
    /// Exclui o registro de uma despesa do banco de dados.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirDespesa(int id)
    {
        var despesa = await context.Despesas.FindAsync(id);
        if (despesa != null)
        {
            context.Despesas.Remove(despesa);
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Despesa excluída com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Despesas" });
    }

    // =========================================================================
    // 4. GESTÃO DE COBRANÇAS DE PACIENTES (RECEITAS)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cobranca cobranca)
    {
        if (ModelState.IsValid)
        {
            context.Cobrancas.Add(cobranca);
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Cobrança cadastrada com sucesso!";
            return RedirectToAction(nameof(Index), new { aba = "Receitas" });
        }
        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", cobranca.PacienteId);
        return View(cobranca);
    }

    public async Task<IActionResult> MarcarComoPago(int id)
    {
        var cobranca = await context.Cobrancas.FindAsync(id);
        if (cobranca != null)
        {
            cobranca.Status = "Pago";
            cobranca.DataPagamento = DateTime.Today;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Cobrança baixada com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Receitas" });
    }

    [HttpGet]
    public async Task<IActionResult> Baixa(int? id)
    {
        if (id == null) return NotFound();
        var cobranca = await context.Cobrancas.Include(c => c.Paciente).FirstOrDefaultAsync(c => c.IdCobranca == id);
        if (cobranca == null) return NotFound();
        cobranca.DataPagamento = DateTime.Today;
        return View(cobranca);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Baixa(int id, Cobranca cobranca)
    {
        if (id != cobranca.IdCobranca) return NotFound();
        var cobrancaOriginal = await context.Cobrancas.FindAsync(id);
        if (cobrancaOriginal != null)
        {
            cobrancaOriginal.Status = "Pago";
            cobrancaOriginal.DataPagamento = cobranca.DataPagamento ?? DateTime.Today;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Pagamento registrado com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Receitas" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var cobranca = await context.Cobrancas.FindAsync(id);
        if (cobranca == null) return NotFound();
        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", cobranca.PacienteId);
        return View(cobranca);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cobranca cobranca)
    {
        if (id != cobranca.IdCobranca) return NotFound();
        if (ModelState.IsValid)
        {
            var cobrancaOriginal = await context.Cobrancas.FindAsync(id);
            if (cobrancaOriginal == null) return NotFound();
            cobrancaOriginal.Descricao = cobranca.Descricao;
            cobrancaOriginal.Valor = cobranca.Valor;
            cobrancaOriginal.DataVencimento = cobranca.DataVencimento;
            cobrancaOriginal.Status = cobranca.Status;
            cobrancaOriginal.DataPagamento = cobranca.DataPagamento;
            cobrancaOriginal.PacienteId = cobranca.PacienteId;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Cobrança atualizada com sucesso!";
            return RedirectToAction(nameof(Index), new { aba = "Receitas" });
        }
        ViewBag.Pacientes = new SelectList(await context.Pacientes.OrderBy(p => p.Nome).ToListAsync(), "IdPaciente", "Nome", cobranca.PacienteId);
        return View(cobranca);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var cobranca = await context.Cobrancas.Include(c => c.Paciente).FirstOrDefaultAsync(m => m.IdCobranca == id);
        return cobranca == null ? NotFound() : View(cobranca);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cobranca = await context.Cobrancas.FindAsync(id);
        if (cobranca != null)
        {
            context.Cobrancas.Remove(cobranca);
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Cobrança excluída com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Receitas" });
    }

    public async Task<IActionResult> Reabrir(int id)
    {
        var cobranca = await context.Cobrancas.FindAsync(id);
        if (cobranca != null)
        {
            cobranca.Status = "Pendente";
            cobranca.DataPagamento = null;
            await context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Cobrança reaberta com sucesso!";
        }
        return RedirectToAction(nameof(Index), new { aba = "Receitas" });
    }

    // =========================================================================
    // 5. EXPORTAÇÕES (EXCEL E PDF)
    // =========================================================================

    /// <summary>
    /// Gera planilha Excel formatada (.xlsx) com as cobranças filtradas.
    /// </summary>
    public IActionResult ExportarExcel(int? pacienteId, string? status, string? competencia)
    {
        var cobrancas = ObterCobrancasFiltradas(pacienteId, status, competencia);

        using var workbook = new XLWorkbook();
        var planilha = workbook.Worksheets.Add("Financeiro");

        planilha.Cell(1, 1).Value = "Vencimento";
        planilha.Cell(1, 2).Value = "Paciente";
        planilha.Cell(1, 3).Value = "Descrição";
        planilha.Cell(1, 4).Value = "Valor";
        planilha.Cell(1, 5).Value = "Status";
        planilha.Cell(1, 6).Value = "Data do Pagamento";

        var linhaCabecalho = planilha.Row(1);
        linhaCabecalho.Style.Font.Bold = true;
        linhaCabecalho.Style.Fill.BackgroundColor = XLColor.FromHtml("#071A3A");
        linhaCabecalho.Style.Font.FontColor = XLColor.White;

        int linha = 2;
        foreach (var c in cobrancas)
        {
            planilha.Cell(linha, 1).Value = c.DataVencimento;
            planilha.Cell(linha, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            planilha.Cell(linha, 2).Value = c.Paciente != null ? c.Paciente.Nome : "Excluído";
            planilha.Cell(linha, 3).Value = c.Descricao;
            planilha.Cell(linha, 4).Value = c.Valor;
            planilha.Cell(linha, 4).Style.NumberFormat.Format = "R$ #,##0.00";
            planilha.Cell(linha, 5).Value = c.Status;
            planilha.Cell(linha, 6).Value = c.DataPagamento.HasValue ? c.DataPagamento.Value.ToString("dd/MM/yyyy") : "-";
            linha++;
        }

        planilha.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Emite relatório financeiro em PDF de alta qualidade com QuestPDF.
    /// </summary>
    public IActionResult ExportarPdf(int? pacienteId, string? status, string? competencia)
    {
        var cobrancas = ObterCobrancasFiltradas(pacienteId, status, competencia);
        var culturaBr = new CultureInfo("pt-BR");

        Paciente? paciente = pacienteId is > 0 ? context.Pacientes.Find(pacienteId.Value) : null;

        var totalGeral = cobrancas.Sum(c => c.Valor);
        var totalPago = cobrancas.Where(c => c.Status == "Pago").Sum(c => c.Valor);
        var totalEmAberto = cobrancas.Where(c => c.Status == "Pendente" || c.Status == "Atrasado").Sum(c => c.Valor);
        var totalCancelado = cobrancas.Where(c => c.Status == "Cancelado").Sum(c => c.Valor);

        string logoPath = Path.Combine(hostEnvironment.WebRootPath, "images", "logo-principal-cerebro-coracao 2.png");
        byte[]? logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                // 1. Cabeçalho Timbrado
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.RelativeItem(7).Row(brandRow =>
                        {
                            if (logoBytes != null)
                            {
                                brandRow.ConstantItem(44).Height(44).Image(logoBytes).FitArea();
                                brandRow.ConstantItem(10);
                            }
                            brandRow.RelativeItem().Column(brandCol =>
                            {
                                brandCol.Item().Row(logoRow =>
                                {
                                    logoRow.AutoItem().Text("Neuro").FontSize(22).Bold().FontColor(Color.FromHex("#071A3A"));
                                    logoRow.AutoItem().Text("Sync").FontSize(22).Bold().FontColor(Color.FromHex("#315BEF"));
                                });
                                brandCol.Item().Text("Gestão Clínica e Prontuário Eletrônico").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                            });
                        });

                        row.RelativeItem(5).AlignRight().Column(metaCol =>
                        {
                            metaCol.Item().Text("RELATÓRIO FINANCEIRO").FontSize(11).Bold().FontColor(Color.FromHex("#071A3A"));
                            metaCol.Item().Text($"Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    headerCol.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Color.FromHex("#315BEF"));
                });

                // 2. Conteúdo e Tabela
                page.Content().PaddingTop(12).Column(contentCol =>
                {
                    // Cards de Totais
                    contentCol.Item().Row(kpiRow =>
                    {
                        kpiRow.RelativeItem().Background(Color.FromHex("#F8FAFC")).Border(1).BorderColor(Color.FromHex("#E2E8F0")).Padding(8).Column(c =>
                        {
                            c.Item().Text("FATURAMENTO").FontSize(7.5f).Bold().FontColor(Colors.Grey.Darken1);
                            c.Item().Text(totalGeral.ToString("C", culturaBr)).FontSize(12).Bold().FontColor(Color.FromHex("#071A3A"));
                        });
                        kpiRow.ConstantItem(8);
                        kpiRow.RelativeItem().Background(Color.FromHex("#F0FDF4")).Border(1).BorderColor(Color.FromHex("#BBF7D0")).Padding(8).Column(c =>
                        {
                            c.Item().Text("RECEBIDO").FontSize(7.5f).Bold().FontColor(Color.FromHex("#15803D"));
                            c.Item().Text(totalPago.ToString("C", culturaBr)).FontSize(12).Bold().FontColor(Color.FromHex("#15803D"));
                        });
                        kpiRow.ConstantItem(8);
                        kpiRow.RelativeItem().Background(Color.FromHex("#FEFCE8")).Border(1).BorderColor(Color.FromHex("#FEF08A")).Padding(8).Column(c =>
                        {
                            c.Item().Text("A RECEBER").FontSize(7.5f).Bold().FontColor(Color.FromHex("#A16207"));
                            c.Item().Text(totalEmAberto.ToString("C", culturaBr)).FontSize(12).Bold().FontColor(Color.FromHex("#A16207"));
                        });
                        kpiRow.ConstantItem(8);
                        kpiRow.RelativeItem().Background(Color.FromHex("#FFF1F2")).Border(1).BorderColor(Color.FromHex("#FECDD3")).Padding(8).Column(c =>
                        {
                            c.Item().Text("CANCELADO").FontSize(7.5f).Bold().FontColor(Color.FromHex("#BE123C"));
                            c.Item().Text(totalCancelado.ToString("C", culturaBr)).FontSize(12).Bold().FontColor(Color.FromHex("#BE123C"));
                        });
                    });

                    // Tabela de Cobranças
                    contentCol.Item().PaddingTop(14).Table(tabela =>
                    {
                        tabela.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(65);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(75);
                            columns.ConstantColumn(75);
                        });

                        tabela.Header(header =>
                        {
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).Text("Vencimento").Bold().FontColor(Colors.White);
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).Text("Paciente").Bold().FontColor(Colors.White);
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).Text("Descrição").Bold().FontColor(Colors.White);
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).AlignRight().Text("Valor").Bold().FontColor(Colors.White);
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).AlignCenter().Text("Status").Bold().FontColor(Colors.White);
                            header.Cell().Background(Color.FromHex("#071A3A")).Padding(6).AlignCenter().Text("Pagamento").Bold().FontColor(Colors.White);
                        });

                        int idx = 0;
                        foreach (var c in cobrancas)
                        {
                            var fundo = idx % 2 == 0 ? Colors.White : Color.FromHex("#F8FAFC");
                            tabela.Cell().Background(fundo).Padding(5).Text(c.DataVencimento.ToString("dd/MM/yyyy"));
                            tabela.Cell().Background(fundo).Padding(5).Text(c.Paciente?.Nome ?? "-").SemiBold();
                            tabela.Cell().Background(fundo).Padding(5).Text(c.Descricao ?? "Atendimento Clínico");
                            tabela.Cell().Background(fundo).Padding(5).AlignRight().Text(c.Valor.ToString("C", culturaBr)).Bold();
                            tabela.Cell().Background(fundo).Padding(5).AlignCenter().Text(c.Status);
                            tabela.Cell().Background(fundo).Padding(5).AlignCenter().Text(c.DataPagamento.HasValue ? c.DataPagamento.Value.ToString("dd/MM/yyyy") : "-");
                            idx++;
                        }
                    });
                });

                // 3. Rodapé
                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("NeuroSync • Gestão Clínica Integrada").FontSize(8).FontColor(Colors.Grey.Medium);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        });

        var pdfBytes = documento.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"RelatorioFinanceiro_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    // =========================================================================
    // 6. CORES E ÍCONES DE CATEGORIAS
    // =========================================================================

    private static string ObterCorCategoria(string cat) => cat switch
    {
        "Energia Elétrica" => "#F59E0B",
        "Água" => "#06B6D4",
        "Telefonia" => "#8B5CF6",
        "Internet" => "#3B82F6",
        "Material de Escritório" => "#EC4899",
        "Sistemas" => "#6366F1",
        "Aluguel" => "#10B981",
        "Limpeza" => "#14B8A6",
        _ => "#64748B"
    };

    private static string ObterIconeCategoria(string cat) => cat switch
    {
        "Energia Elétrica" => "bi-lightning-charge-fill",
        "Água" => "bi-droplet-fill",
        "Telefonia" => "bi-telephone-fill",
        "Internet" => "bi-wifi",
        "Material de Escritório" => "bi-box-seam-fill",
        "Sistemas" => "bi-laptop",
        "Aluguel" => "bi-building",
        "Limpeza" => "bi-stars",
        _ => "bi-receipt"
    };
}