namespace NeuroSync.Models;

/// <summary>
/// Modelo unificado de exibição para a tela de Gestão Financeira (Abas Resumo, Receitas e Despesas).
/// </summary>
public class FinanceiroViewModel
{
    /// <summary>
    /// Aba selecionada ("Resumo", "Receitas", "Despesas").
    /// </summary>
    public string AbaAtiva { get; set; } = "Resumo";

    // ==========================================
    // DADOS DA ABA RECEITAS (COBRANÇAS)
    // ==========================================
    public List<Cobranca> Cobrancas { get; set; } = [];
    public int? PacienteId { get; set; }
    public string? PacienteNome { get; set; }
    public string StatusSelecionado { get; set; } = "Todos";
    public string CompetenciaSelecionada { get; set; } = string.Empty;

    // KPIs de Receitas
    public decimal ReceitaMes { get; set; }
    public double VariacaoReceitaMes { get; set; }
    public decimal TotalRecebido { get; set; }
    public double PercentualRecebido { get; set; }
    public decimal TotalAReceber { get; set; }
    public double PercentualAReceber { get; set; }
    public double TaxaInadimplencia { get; set; }
    public decimal TotalEmAberto { get; set; }

    // ==========================================
    // DADOS DA ABA DESPESAS
    // ==========================================
    public List<Despesa> Despesas { get; set; } = [];
    public string CategoriaDespesaSelecionada { get; set; } = "Todas";
    public string StatusDespesaSelecionado { get; set; } = "Todos";

    // KPIs de Despesas
    public decimal TotalDespesasMes { get; set; }
    public decimal DespesasMes => TotalDespesasMes;
    public decimal DespesasPagasMes { get; set; }
    public decimal DespesasPendentesMes { get; set; }
    public double VariacaoDespesas { get; set; }
    public string MaiorCategoriaDespesa { get; set; } = "Aluguel";

    // ==========================================
    // DADOS DA ABA RESUMO (CONSOLIDADO)
    // ==========================================
    public decimal SaldoLiquidoMes { get; set; }

    // Gráfico 1: Receita x Despesas (Barras agrupadas nos últimos meses)
    public List<string> MesesLabels { get; set; } = [];
    public List<decimal> ReceitasMensais { get; set; } = [];
    public List<decimal> DespesasMensais { get; set; } = [];

    // Gráfico 2: Despesas por Categoria (Donut)
    public List<string> CategoriasLabels { get; set; } = [];
    public List<decimal> CategoriasValores { get; set; } = [];

    // Movimentações Recentes na Aba Resumo
    public List<Cobranca> UltimasReceitas { get; set; } = [];
    public List<Despesa> ProximasDespesas { get; set; } = [];
}
