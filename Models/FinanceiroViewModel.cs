namespace NeuroSync.Models;
public class FinanceiroViewModel
{
    public string AbaAtiva { get; set; } = "Resumo";

    public List<Cobranca> Cobrancas { get; set; } = [];
    public int? PacienteId { get; set; }
    public string? PacienteNome { get; set; }
    public string StatusSelecionado { get; set; } = "Todos";
    public string CompetenciaSelecionada { get; set; } = string.Empty;

    public decimal ReceitaMes { get; set; }
    public double VariacaoReceitaMes { get; set; }
    public decimal TotalRecebido { get; set; }
    public double PercentualRecebido { get; set; }
    public decimal TotalAReceber { get; set; }
    public double PercentualAReceber { get; set; }
    public double TaxaInadimplencia { get; set; }
    public decimal TotalEmAberto { get; set; }

    public List<Despesa> Despesas { get; set; } = [];
    public string CategoriaDespesaSelecionada { get; set; } = "Todas";
    public string StatusDespesaSelecionado { get; set; } = "Todos";

    public decimal TotalDespesasMes { get; set; }
    public decimal DespesasMes => TotalDespesasMes;
    public decimal DespesasPagasMes { get; set; }
    public decimal DespesasPendentesMes { get; set; }
    public double VariacaoDespesas { get; set; }
    public string MaiorCategoriaDespesa { get; set; } = "Aluguel";

    public decimal SaldoLiquidoMes { get; set; }

    public List<string> MesesLabels { get; set; } = [];
    public List<decimal> ReceitasMensais { get; set; } = [];
    public List<decimal> DespesasMensais { get; set; } = [];

    public List<string> CategoriasLabels { get; set; } = [];
    public List<decimal> CategoriasValores { get; set; } = [];

    public List<Cobranca> UltimasReceitas { get; set; } = [];
    public List<Despesa> ProximasDespesas { get; set; } = [];
}
