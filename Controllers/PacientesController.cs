using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO; 
using System.Globalization;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Hosting; 
using Microsoft.EntityFrameworkCore;
using NeuroSync.Models;
using NeuroSync.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NeuroSync.Controllers
{
    [Authorize]
    public class PacientesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // O Construtor recebe o banco de dados e o controle de pastas do servidor
        public PacientesController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. TELA DE BUSCA E LISTAGEM
        public IActionResult Index(string termoBusca)
        {
            var pacientes = _context.Pacientes.AsQueryable();
            if (!string.IsNullOrEmpty(termoBusca))
            {
                pacientes = pacientes.Where(p => p.Nome.ToLower().Contains(termoBusca.ToLower()));
                ViewBag.BuscaAtual = termoBusca;
            }
            return View(pacientes.OrderBy(p => p.Nome).ToList());
        }

        // 2. TELA DE PRONTUÁRIO / PERFIL (Trazendo Agendamentos, Evoluções, Pareceres e Anexos)
        public async Task<IActionResult> Details(int? id, string? aba = null)
        {
            if (id == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(m => m.IdPaciente == id);
            if (paciente == null) return NotFound();

            // Busca Evoluções
            var evolucoes = await _context.Evolucoes
                                          .Where(e => e.PacienteId == id)
                                          .OrderByDescending(e => e.DataRegistro)
                                          .ToListAsync();
            
            ViewBag.Evolucoes = evolucoes;
            ViewBag.UltimasEvolucoes = evolucoes;

            // Busca Todos os Agendamentos do Paciente
            var todosAgendamentos = await _context.Agendamentos
                                                 .Where(a => a.PacienteId == id)
                                                 .OrderByDescending(a => a.DataHora)
                                                 .ToListAsync();
            
            ViewBag.TodosAgendamentos = todosAgendamentos;
            ViewBag.Agendamentos = todosAgendamentos.Where(a => a.DataHora >= DateTime.Today).OrderBy(a => a.DataHora).ToList();
            ViewBag.ProximosAtendimentos = ViewBag.Agendamentos;

            // Busca Pareceres Técnicos Emitidos
            var pareceres = await _context.PareceresTecnicos
                                          .Where(p => p.PacienteId == id)
                                          .OrderByDescending(p => p.DataEmissao)
                                          .ToListAsync();
            ViewBag.PareceresTecnicos = pareceres;

            // Busca Cobranças do Paciente
            var cobrancas = await _context.Cobrancas
                                          .Where(c => c.PacienteId == id)
                                          .OrderByDescending(c => c.DataVencimento)
                                          .ToListAsync();
            ViewBag.Cobrancas = cobrancas;

            // Busca Arquivos Anexos
            ViewBag.Anexos = await _context.Anexos
                                           .Where(a => a.PacienteId == id)
                                           .OrderByDescending(a => a.DataUpload)
                                           .ToListAsync();

            ViewBag.AbaAtiva = string.IsNullOrWhiteSpace(aba) ? "resumo" : aba.ToLower().Trim();

            return View(paciente);
        }

        // 3. SALVAR EVOLUÇÃO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarEvolucao(int PacienteId, string Anotacao, string? TipoEvolucao, string? ProfissionalNome)
        {
            if (!string.IsNullOrWhiteSpace(Anotacao))
            {
                var tipo = !string.IsNullOrWhiteSpace(TipoEvolucao) ? TipoEvolucao.Trim() : "Sessão Terapêutica";
                var profissional = !string.IsNullOrWhiteSpace(ProfissionalNome)
                    ? ProfissionalNome.Trim()
                    : (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name) ? User.Identity.Name : "Profissional Clínico");

                var novaEvolucao = new Evolucao
                {
                    PacienteId = PacienteId,
                    Anotacao = Anotacao.Trim(),
                    TipoEvolucao = tipo,
                    ProfissionalNome = profissional,
                    DataRegistro = DateTime.Now
                };
                _context.Evolucoes.Add(novaEvolucao);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Evolução clínica registrada com sucesso!";
                TempData["MensagemSucessoEvolucao"] = "Evolução clínica registrada com sucesso!";
            }
            return RedirectToAction("Details", new { id = PacienteId, aba = "evolucao" });
        }

        // ========================================================
        // 3.01 VISUALIZAR DETALHES DA EVOLUÇÃO
        // ========================================================
        public async Task<IActionResult> VisualizarEvolucao(int id)
        {
            var evolucao = await _context.Evolucoes
                                         .Include(e => e.Paciente)
                                         .FirstOrDefaultAsync(e => e.IdEvolucao == id);

            if (evolucao == null || evolucao.Paciente == null) return NotFound();

            return View(evolucao);
        }

        // ========================================================
        // 3.02 IMPRIMIR EVOLUÇÃO (FOLHA TIMBRADA WEB)
        // ========================================================
        public async Task<IActionResult> ImprimirEvolucao(int id)
        {
            var evolucao = await _context.Evolucoes
                                         .Include(e => e.Paciente)
                                         .FirstOrDefaultAsync(e => e.IdEvolucao == id);

            if (evolucao == null || evolucao.Paciente == null) return NotFound();

            return View(evolucao);
        }

        // ========================================================
        // 3.03 EMISSÃO DO PDF DA EVOLUÇÃO CLÍNICA COM LOGO (QuestPDF)
        // ========================================================
        public async Task<IActionResult> GerarEvolucaoPdf(int id)
        {
            var evolucao = await _context.Evolucoes
                                         .Include(e => e.Paciente)
                                         .FirstOrDefaultAsync(e => e.IdEvolucao == id);

            if (evolucao == null || evolucao.Paciente == null) return NotFound();

            var paciente = evolucao.Paciente;
            var culturaBr = new CultureInfo("pt-BR");

            // Cálculo seguro de idade
            var idade = DateTime.Today.Year - paciente.DataNascimento.Year;
            if (paciente.DataNascimento.Date > DateTime.Today.AddYears(-idade)) idade--;

            // Identificação do Responsável
            string responsavel = !string.IsNullOrWhiteSpace(paciente.Responsavel) ? paciente.Responsavel :
                                 !string.IsNullOrWhiteSpace(paciente.NomeMae) ? paciente.NomeMae :
                                 !string.IsNullOrWhiteSpace(paciente.NomePai) ? paciente.NomePai : "Não informado";

            // Imagem do Logo Principal
            string logoPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logo-principal-cerebro-coracao 2.png");
            byte[]? logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(Color.FromHex("#1e293b")));

                    // 1. CABEÇALHO TIMBRADO
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            row.RelativeItem(7).Row(bRow =>
                            {
                                if (logoBytes != null)
                                {
                                    bRow.ConstantItem(46).Height(46).Image(logoBytes).FitArea();
                                    bRow.ConstantItem(10);
                                }

                                bRow.RelativeItem().Column(brandCol =>
                                {
                                    brandCol.Item().Row(logoRow =>
                                    {
                                        logoRow.AutoItem().Text("Neuro").FontSize(20).Bold().FontColor(Color.FromHex("#071A3A"));
                                        logoRow.AutoItem().Text("Sync").FontSize(20).Bold().FontColor(Color.FromHex("#315BEF"));
                                    });
                                    brandCol.Item().Text("Clínica de Desenvolvimento e Neuropsicopedagogia").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                                    brandCol.Item().Text("Atendimento Clínico Multidisciplinar Especializado").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                                });
                            });

                            row.RelativeItem(5).AlignRight().Column(metaCol =>
                            {
                                metaCol.Item().Text("REGISTRO DE EVOLUÇÃO").FontSize(12).Bold().FontColor(Color.FromHex("#071A3A"));
                                metaCol.Item().Text($"Registro: #EV{evolucao.IdEvolucao:D4}").FontSize(8.5f).FontColor(Color.FromHex("#315BEF")).Bold();
                                metaCol.Item().Text($"Data: {evolucao.DataRegistro:dd/MM/yyyy 'às' HH:mm}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                            });
                        });

                        headerCol.Item().PaddingTop(8).LineHorizontal(2).LineColor(Color.FromHex("#315BEF"));
                    });

                    // 2. CORPO DO DOCUMENTO
                    page.Content().PaddingTop(16).Column(col =>
                    {
                        // 2.1 Identificação do Paciente
                        col.Item().Background(Color.FromHex("#F8FAFC")).Border(1).BorderColor(Color.FromHex("#E2E8F0")).Padding(12).Column(pCol =>
                        {
                            pCol.Item().Text("IDENTIFICAÇÃO DO PACIENTE").FontSize(8.5f).Bold().FontColor(Color.FromHex("#315BEF"));
                            pCol.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem(6).Text(t =>
                                {
                                    t.Span("Nome do Paciente: ").Bold();
                                    t.Span(paciente.Nome);
                                });
                                r.RelativeItem(3).Text(t =>
                                {
                                    t.Span("Idade: ").Bold();
                                    t.Span($"{idade} anos");
                                });
                                r.RelativeItem(3).Text(t =>
                                {
                                    t.Span("Prontuário: ").Bold();
                                    t.Span($"#{paciente.IdPaciente:D4}");
                                });
                            });

                            pCol.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem(6).Text(t =>
                                {
                                    t.Span("Responsável: ").Bold();
                                    t.Span(responsavel);
                                });
                                r.RelativeItem(6).Text(t =>
                                {
                                    t.Span("Diagnóstico/CID: ").Bold();
                                    t.Span(!string.IsNullOrWhiteSpace(paciente.DiagnosticoPrincipal) ? paciente.DiagnosticoPrincipal : "Em investigação");
                                });
                            });
                        });

                        // 2.2 Dados do Atendimento
                        col.Item().PaddingTop(12).Background(Color.FromHex("#EFF6FF")).Border(1).BorderColor(Color.FromHex("#BFDBFE")).Padding(10).Row(r =>
                        {
                            r.RelativeItem(4).Text(t =>
                            {
                                t.Span("Tipo de Atendimento: ").Bold();
                                t.Span(!string.IsNullOrWhiteSpace(evolucao.TipoEvolucao) ? evolucao.TipoEvolucao : "Sessão Terapêutica");
                            });
                            r.RelativeItem(4).Text(t =>
                            {
                                t.Span("Data e Horário: ").Bold();
                                t.Span(evolucao.DataRegistro.ToString("dd/MM/yyyy HH:mm", culturaBr));
                            });
                            r.RelativeItem(4).Text(t =>
                            {
                                t.Span("Profissional: ").Bold();
                                t.Span(!string.IsNullOrWhiteSpace(evolucao.ProfissionalNome) ? evolucao.ProfissionalNome : "Especialista Clínico");
                            });
                        });

                        // 2.3 Conteúdo da Evolução
                        col.Item().PaddingTop(16).Column(relatoCol =>
                        {
                            relatoCol.Item().Row(r =>
                            {
                                r.AutoItem().Text("DESCRIÇÃO DA SESSÃO & CONDUTAS ADOTADAS").FontSize(10).Bold().FontColor(Color.FromHex("#071A3A"));
                            });
                            relatoCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Color.FromHex("#CBD5E1"));

                            relatoCol.Item().PaddingTop(10).Text(evolucao.Anotacao)
                                .FontSize(10)
                                .FontColor(Color.FromHex("#0f172a"));
                        });

                        // 2.4 Campo de Assinatura
                        col.Item().PaddingTop(40).AlignCenter().Column(signCol =>
                        {
                            signCol.Item().Width(240).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            signCol.Item().PaddingTop(4).Text(!string.IsNullOrWhiteSpace(evolucao.ProfissionalNome) ? evolucao.ProfissionalNome : "Profissional Responsável")
                                .Bold().FontSize(9.5f);
                            signCol.Item().Text("NeuroSync - Gestão Clínica Multidisciplinar")
                                .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // 3. RODAPÉ TIMBRADO
                    page.Footer().Column(fCol =>
                    {
                        fCol.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                        fCol.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem(8).Text("NeuroSync Gestão Clínica • Registro Eletrônico de Saúde do Paciente").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            r.RelativeItem(4).AlignRight().Text(x =>
                            {
                                x.Span("Página ").FontSize(7.5f);
                                x.CurrentPageNumber().FontSize(7.5f);
                                x.Span(" de ").FontSize(7.5f);
                                x.TotalPages().FontSize(7.5f);
                            });
                        });
                    });
                });
            });

            byte[] pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Evolucao-{paciente.Nome.Replace(" ", "_")}-{evolucao.DataRegistro:yyyyMMdd}.pdf");
        }

        // ========================================================
        // 3.1 SALVAR PARECER TÉCNICO
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarParecerTecnico(ParecerTecnico model)
        {
            if (model.PacienteId <= 0) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Titulo))
            {
                model.Titulo = "Relatório de Avaliação Neuropsicopedagógica";
            }

            if (model.DataEmissao == default)
            {
                model.DataEmissao = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                if (model.IdParecer == 0)
                {
                    _context.PareceresTecnicos.Add(model);
                }
                else
                {
                    _context.PareceresTecnicos.Update(model);
                }
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Parecer técnico gerado com sucesso! O documento já está disponível para consulta e emissão do PDF.";
                return RedirectToAction("Details", new { id = model.PacienteId, aba = "parecer" });
            }

            TempData["MensagemErro"] = "Por favor, verifique os campos do parecer.";
            return RedirectToAction("Details", new { id = model.PacienteId, aba = "parecer" });
        }

        // ========================================================
        // 3.2 EMISSÃO DO PDF DO PARECER TÉCNICO COM LOGO
        // ========================================================
        public async Task<IActionResult> GerarParecerPdf(int id)
        {
            var parecer = await _context.PareceresTecnicos
                                        .Include(p => p.Paciente)
                                        .FirstOrDefaultAsync(p => p.IdParecer == id);

            if (parecer == null || parecer.Paciente == null) return NotFound();

            var paciente = parecer.Paciente;
            var culturaBr = new CultureInfo("pt-BR");

            // Cálculo seguro de idade
            var idade = DateTime.Today.Year - paciente.DataNascimento.Year;
            if (paciente.DataNascimento.Date > DateTime.Today.AddYears(-idade)) idade--;

            // Identificação do Responsável
            string responsavel = !string.IsNullOrWhiteSpace(paciente.Responsavel) ? paciente.Responsavel :
                                 !string.IsNullOrWhiteSpace(paciente.NomeMae) ? paciente.NomeMae :
                                 !string.IsNullOrWhiteSpace(paciente.NomePai) ? paciente.NomePai : "Não informado";

            // Imagem do Logo Principal
            string logoPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logo-principal-cerebro-coracao 2.png");
            byte[]? logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(Color.FromHex("#1e293b")));

                    // 1. CABEÇALHO DO LAUDO
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            // Logo e Identificação Clínica
                            row.RelativeItem(7).Row(bRow =>
                            {
                                if (logoBytes != null)
                                {
                                    bRow.ConstantItem(46).Height(46).Image(logoBytes).FitArea();
                                    bRow.ConstantItem(10);
                                }

                                bRow.RelativeItem().Column(brandCol =>
                                {
                                    brandCol.Item().Row(logoRow =>
                                    {
                                        logoRow.AutoItem().Text("Neuro").FontSize(20).Bold().FontColor(Color.FromHex("#071A3A"));
                                        logoRow.AutoItem().Text("Sync").FontSize(20).Bold().FontColor(Color.FromHex("#315BEF"));
                                    });
                                    brandCol.Item().Text("Clínica de Desenvolvimento e Neuropsicopedagogia").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                                    brandCol.Item().Text("Atendimento Clínico Multidisciplinar Especializado").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                                });
                            });

                            // Tipo e Emissão
                            row.RelativeItem(5).AlignRight().Column(metaCol =>
                            {
                                metaCol.Item().Text("PARECER TÉCNICO").FontSize(12).Bold().FontColor(Color.FromHex("#071A3A"));
                                metaCol.Item().Text($"Registro Clínico: #P{parecer.IdParecer:D4}").FontSize(8.5f).FontColor(Color.FromHex("#315BEF")).Bold();
                                metaCol.Item().Text($"Emissão: {parecer.DataEmissao:dd/MM/yyyy}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                            });
                        });

                        // Linha decorativa
                        headerCol.Item().PaddingTop(8).LineHorizontal(2).LineColor(Color.FromHex("#315BEF"));
                    });

                    // 2. CONTEÚDO DO PARECER
                    page.Content().PaddingTop(12).Column(contentCol =>
                    {
                        // 2.1 IDENTIFICAÇÃO DO PACIENTE (CARD ELEGANTE)
                        contentCol.Item().Border(1).BorderColor(Color.FromHex("#e2e8f0")).Background(Color.FromHex("#f8fafc")).Padding(10).Column(pCol =>
                        {
                            pCol.Item().Row(pRow =>
                            {
                                pRow.RelativeItem(6).Text(t =>
                                {
                                    t.Span("Paciente: ").Bold().FontColor(Color.FromHex("#0f172a"));
                                    t.Span(paciente.Nome).Bold().FontColor(Color.FromHex("#1e40af"));
                                });

                                pRow.RelativeItem(3).Text(t =>
                                {
                                    t.Span("Idade: ").Bold().FontColor(Color.FromHex("#0f172a"));
                                    t.Span($"{idade} anos ({paciente.DataNascimento:dd/MM/yyyy})");
                                });

                                pRow.RelativeItem(3).AlignRight().Text(t =>
                                {
                                    t.Span("CPF: ").Bold().FontColor(Color.FromHex("#0f172a"));
                                    t.Span(!string.IsNullOrWhiteSpace(paciente.Cpf) ? paciente.Cpf : "-");
                                });
                            });

                            pCol.Item().PaddingTop(4).Row(pRow2 =>
                            {
                                pRow2.RelativeItem(6).Text(t =>
                                {
                                    t.Span("Responsável: ").Bold().FontColor(Color.FromHex("#0f172a"));
                                    t.Span(responsavel);
                                });

                                pRow2.RelativeItem(6).AlignRight().Text(t =>
                                {
                                    t.Span("Profissional: ").Bold().FontColor(Color.FromHex("#0f172a"));
                                    t.Span($"{parecer.ProfissionalNome} ({parecer.RegistroProfissional})");
                                });
                            });
                        });

                        // 2.2 TÍTULO CENTRAL DO DOCUMENTO
                        contentCol.Item().PaddingTop(12).PaddingBottom(6).AlignCenter().Text(parecer.Titulo.ToUpper())
                                  .FontSize(12).Bold().FontColor(Color.FromHex("#071A3A"));

                        // 2.3 SEÇÕES CLÍNICAS PADRONIZADAS (CONFORME FORMULÁRIO)
                        Action<string, string, string?> renderSecao = (numero, titulo, texto) =>
                        {
                            if (!string.IsNullOrWhiteSpace(texto))
                            {
                                contentCol.Item().PaddingTop(10).Column(sCol =>
                                {
                                    sCol.Item().Row(sRow =>
                                    {
                                        sRow.AutoItem().Text(numero).Bold().FontColor(Color.FromHex("#315BEF")).FontSize(10.5f);
                                        sRow.ConstantItem(6);
                                        sRow.AutoItem().Text(titulo.ToUpper()).Bold().FontColor(Color.FromHex("#0f172a")).FontSize(10f);
                                    });
                                    sCol.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Color.FromHex("#cbd5e1"));
                                    sCol.Item().PaddingTop(5).Text(texto).FontSize(9f).LineHeight(1.35f);
                                });
                            }
                        };

                        renderSecao("1.", "Motivo da Avaliação", parecer.MotivoAvaliacao);
                        renderSecao("2.", "Procedimentos e Recursos Utilizados", parecer.ProcedimentosRecursos);
                        renderSecao("3.", "Análise do Processo Avaliativo", parecer.AnaliseAvaliativa);
                        renderSecao("4.", "Síntese Avaliativa e Diagnóstica", parecer.SinteseAvaliativa);
                        renderSecao("5.", "Recomendações e Considerações Finais", parecer.RecomendacoesFinais);

                        // 2.4 ASSINATURA DO PROFISSIONAL
                        contentCol.Item().PaddingTop(30).AlignCenter().Column(sigCol =>
                        {
                            sigCol.Item().Width(260).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                            sigCol.Item().PaddingTop(4).AlignCenter().Text(parecer.ProfissionalNome).Bold().FontSize(10.5f).FontColor(Color.FromHex("#071A3A"));
                            sigCol.Item().AlignCenter().Text(parecer.RegistroProfissional ?? "Especialista em Desenvolvimento Humano").FontSize(9).FontColor(Colors.Grey.Darken2);
                            sigCol.Item().PaddingTop(2).AlignCenter().Text($"Emitido em {parecer.DataEmissao.ToString("dd 'de' MMMM 'de' yyyy", culturaBr)}").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                        });

                        // 2.5 AVISO DE SIGILO E CONFIDENCIALIDADE
                        contentCol.Item().PaddingTop(18).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Column(avisoCol =>
                        {
                            avisoCol.Item().Text(t =>
                            {
                                t.Span("CONFIDENCIAL: ").Bold().FontSize(7.5f).FontColor(Colors.Grey.Darken3);
                                t.Span("Este documento é estritamente confidencial e de uso exclusivo para fins clínicos e terapêuticos, respaldado pelo sigilo profissional. É vedada sua reprodução para terceiros sem autorização prévia por escrito.").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                            });
                        });
                    });

                    // 3. RODAPÉ INSTITUCIONAL
                    page.Footer().Column(footCol =>
                    {
                        footCol.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        footCol.Item().PaddingTop(5).Row(footRow =>
                        {
                            footRow.RelativeItem().Text($"NeuroSync Gestão Clínica • Paciente: {paciente.Nome}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            footRow.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Página ").FontSize(7.5f);
                                x.CurrentPageNumber().FontSize(7.5f);
                                x.Span(" de ").FontSize(7.5f);
                                x.TotalPages().FontSize(7.5f);
                            });
                        });
                    });
                });
            });

            var pdfBytes = documento.GeneratePdf();
            var nomeSanitizado = string.Join("_", paciente.Nome.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
            string nomeArquivo = $"Parecer_Tecnico_{nomeSanitizado}_{parecer.IdParecer}.pdf";

            return File(pdfBytes, "application/pdf", nomeArquivo);
        }

        // ========================================================
        // 4. UPLOAD DE ANEXOS (PDF/IMAGEM)
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> UploadArquivo(int PacienteId, IFormFile arquivoUpload)
        {
            if (arquivoUpload != null && arquivoUpload.Length > 0)
            {
                // 1. Descobre onde é a pasta wwwroot/uploads
                string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                
                // 2. Se a pasta não existir, cria ela automaticamente
                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                // 3. Cria um nome único para o arquivo não substituir outro
                string nomeUnico = Guid.NewGuid().ToString() + "_" + arquivoUpload.FileName;
                string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                // 4. Copia o arquivo do computador para dentro da pasta do sistema
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivoUpload.CopyToAsync(stream);
                }

                // 5. Salva o registro no Banco de Dados
                var novoAnexo = new Anexo
                {
                    PacienteId = PacienteId,
                    NomeArquivo = arquivoUpload.FileName,
                    CaminhoArquivo = "/uploads/" + nomeUnico, // Rota para acessar na web
                    DataUpload = DateTime.Now
                };

                _context.Anexos.Add(novoAnexo);
                await _context.SaveChangesAsync();
            }

            // Volta para a tela do paciente atualizada
            return RedirectToAction("Details", new { id = PacienteId });
        }

        // 5. CADASTRAR NOVO PACIENTE
        public IActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Paciente paciente)
        {
            if (ModelState.IsValid) 
            { 
                _context.Add(paciente); 
                await _context.SaveChangesAsync(); 
                return RedirectToAction("Index"); 
            }
            return View(paciente);
        }

        // 6. EDITAR PACIENTE E ANAMNESE
        public IActionResult Edit(int? id, string aba = "resumo")
        {
            if (id == null) return NotFound();
            var paciente = _context.Pacientes.Find(id);
            if (paciente == null) return NotFound();
            ViewBag.AbaAtiva = string.IsNullOrEmpty(aba) ? "resumo" : aba.ToLower();
            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Paciente paciente, string abaAtiva = "resumo")
        {
            if (id != paciente.IdPaciente) return NotFound();
            
            if (ModelState.IsValid) 
            { 
                _context.Update(paciente); 
                await _context.SaveChangesAsync(); 
                return RedirectToAction("Details", new { id = paciente.IdPaciente, aba = !string.IsNullOrEmpty(abaAtiva) ? abaAtiva : "resumo" }); 
            }
            ViewBag.AbaAtiva = abaAtiva;
            return View(paciente);
        }

        // 7. EXCLUIR PACIENTE
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            var paciente = _context.Pacientes.Find(id);
            if (paciente == null) return NotFound();
            return View(paciente);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var paciente = _context.Pacientes.Find(id);
            if (paciente != null) 
            { 
                _context.Pacientes.Remove(paciente); 
                _context.SaveChanges(); 
            }
            return RedirectToAction("Index");
        }
    }
}