using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NeuroSync.Controllers
{
    [Authorize]
    public class FinanceiroController : Controller
    {
        private readonly AppDbContext _context;

        public FinanceiroController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // FILTRO CENTRAL
        // ==========================================
        private List<Cobranca> ObterCobrancasFiltradas(int? pacienteId, string? status, string? competencia)
        {
            var query = _context.Cobrancas
                                .Include(c => c.Paciente)
                                .Include(c => c.Agendamento)
                                .AsQueryable();

            query = query.Where(c => c.AgendamentoId == null || (c.Agendamento != null && c.Agendamento.Status == "Realizado"));

            if (pacienteId.HasValue && pacienteId.Value > 0)
            {
                query = query.Where(c => c.PacienteId == pacienteId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(competencia) &&
                DateTime.TryParseExact(competencia, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var competenciaData))
            {
                query = query.Where(c => c.DataVencimento.Year == competenciaData.Year && c.DataVencimento.Month == competenciaData.Month);
            }

            return query.OrderBy(c => c.DataVencimento).ToList();
        }

        private void PreencherFiltrosViewBag(int? pacienteId, string? status, string? competencia)
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome", pacienteId);
            ViewBag.StatusSelecionado = status ?? "Todos";
            ViewBag.CompetenciaSelecionada = competencia ?? string.Empty;
            ViewBag.PacienteSelecionado = pacienteId;
        }

        // 1. TELA PRINCIPAL
        public IActionResult Index(int? pacienteId, string? status, string? competencia)
        {
            var cobrancas = ObterCobrancasFiltradas(pacienteId, status, competencia);
            PreencherFiltrosViewBag(pacienteId, status, competencia);

            return View(cobrancas);
        }

        // 2. GET: Nova cobrança
        public IActionResult Create()
        {
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome");
            return View();
        }

        // 3. POST: Salva cobrança
        [HttpPost]
        public IActionResult Create(Cobranca cobranca)
        {
            if (ModelState.IsValid)
            {
                _context.Cobrancas.Add(cobranca);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home"); 
            }
            
            ViewBag.Pacientes = new SelectList(_context.Pacientes.OrderBy(p => p.Nome), "IdPaciente", "Nome", cobranca.PacienteId);
            return View(cobranca);
        }

        // 4. GET: Confirmação de pagamento
        public IActionResult Baixa(int? id)
        {
            if (id == null) return NotFound();

            var cobranca = _context.Cobrancas
                                   .Include(c => c.Paciente)
                                   .Include(c => c.Agendamento)
                                   .FirstOrDefault(c => c.IdCobranca == id);

            if (cobranca == null) return NotFound();

            cobranca.DataPagamento = DateTime.Today;
            ViewBag.SessaoPendente = cobranca.Agendamento != null && cobranca.Agendamento.Status != "Realizado";

            return View(cobranca);
        }

        // 5. POST: Efetiva o pagamento
        [HttpPost]
        public IActionResult Baixa(int id, Cobranca cobranca)
        {
            if (id != cobranca.IdCobranca) return NotFound();

            var cobrancaOriginal = _context.Cobrancas
                                           .Include(c => c.Paciente)
                                           .Include(c => c.Agendamento)
                                           .FirstOrDefault(c => c.IdCobranca == id);

            if (cobrancaOriginal != null)
            {
                if (cobrancaOriginal.Agendamento != null && cobrancaOriginal.Agendamento.Status != "Realizado")
                {
                    ModelState.AddModelError(string.Empty, "Não é possível confirmar o pagamento: a sessão vinculada ainda não foi marcada como \"Realizado\".");
                    ViewBag.SessaoPendente = true;
                    return View(cobrancaOriginal);
                }

                cobrancaOriginal.Status = "Pago";
                cobrancaOriginal.DataPagamento = cobranca.DataPagamento;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // 6. GET: Confirmação para apagar
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var cobranca = _context.Cobrancas
                                   .Include(c => c.Paciente)
                                   .FirstOrDefault(c => c.IdCobranca == id);

            if (cobranca == null) return NotFound();

            return View(cobranca);
        }

        // 7. POST: Apaga a cobrança
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var cobranca = _context.Cobrancas.Find(id);
            
            if (cobranca != null)
            {
                _context.Cobrancas.Remove(cobranca);
                _context.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        // ==========================================
        // 8. EXPORTAÇÃO EXCEL (.xlsx) 
        // ==========================================
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
            linhaCabecalho.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");

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

                if (c.DataPagamento.HasValue)
                {
                    planilha.Cell(linha, 6).Value = c.DataPagamento.Value;
                    planilha.Cell(linha, 6).Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else
                {
                    planilha.Cell(linha, 6).Value = "-";
                }
                linha++;
            }

            planilha.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var conteudo = stream.ToArray();

            var nomeArquivo = $"financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(conteudo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nomeArquivo);
        }

        // ==========================================
        // 9. EXPORTAÇÃO PDF 
        // ==========================================
        public IActionResult ExportarPdf(int? pacienteId, string? status, string? competencia)
        {
            var cobrancas = ObterCobrancasFiltradas(pacienteId, status, competencia);

            string subtitulo = "Todas as cobranças liberadas";
            if (pacienteId.HasValue && pacienteId.Value > 0)
            {
                var paciente = _context.Pacientes.Find(pacienteId.Value);
                if (paciente != null) subtitulo = $"Paciente: {paciente.Nome}";
            }

            var totalGeral = cobrancas.Sum(c => c.Valor);
            var culturaBr = new CultureInfo("pt-BR");

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Relatório Financeiro - NeuroSync").FontSize(18).Bold();
                        col.Item().Text(subtitulo).FontSize(11).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(15).Column(mainCol =>
                    {
                        mainCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Vencimento").Bold();
                                header.Cell().Text("Paciente").Bold();
                                header.Cell().Text("Descrição").Bold();
                                header.Cell().Text("Valor").Bold();
                                header.Cell().Text("Status").Bold();

                                header.Cell().ColumnSpan(5).PaddingBottom(4).BorderBottom(1).BorderColor(Colors.Grey.Darken1);
                            });

                            foreach (var c in cobrancas)
                            {
                                table.Cell().Padding(3).Text(c.DataVencimento.ToString("dd/MM/yyyy"));
                                table.Cell().Padding(3).Text(c.Paciente != null ? c.Paciente.Nome : "Excluído");
                                table.Cell().Padding(3).Text(c.Descricao);
                                table.Cell().Padding(3).Text(c.Valor.ToString("C", culturaBr));
                                table.Cell().Padding(3).Text(c.Status);
                            }
                        });

                        mainCol.Item().PaddingTop(15).AlignRight().Text($"Total: {totalGeral.ToString("C", culturaBr)}").Bold().FontSize(12);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = documento.GeneratePdf();
            var nomeArquivo = $"financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", nomeArquivo);
        }
    }
}