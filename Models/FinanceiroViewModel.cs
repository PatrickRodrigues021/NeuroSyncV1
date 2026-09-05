using System.Collections.Generic;

namespace NeuroSync.Models
{
    public class FinanceiroViewModel
    {
        // Cobranças filtradas para a tabela
        public List<Cobranca> Cobrancas { get; set; } = new();

        // Filtros selecionados
        public int? PacienteId { get; set; }
        public string? PacienteNome { get; set; }
        public string StatusSelecionado { get; set; } = "Todos";
        public string CompetenciaSelecionada { get; set; } = string.Empty;
        public string AbaAtiva { get; set; } = "Resumo";

        // KPIs do Cabeçalho
        public decimal ReceitaMes { get; set; }
        public double VariacaoReceitaMes { get; set; }

        public decimal TotalRecebido { get; set; }
        public double PercentualRecebido { get; set; }

        public decimal TotalAReceber { get; set; }
        public double PercentualAReceber { get; set; }

        public decimal DespesasMes { get; set; }
        public double VariacaoDespesas { get; set; }

        // Gráfico 1: Receita x Despesas (Barras agrupadas)
        public List<string> MesesLabels { get; set; } = new();
        public List<decimal> ReceitasMensais { get; set; } = new();
        public List<decimal> DespesasMensais { get; set; } = new();

        // Gráfico 2: Inadimplência (Donut)
        public double TaxaInadimplencia { get; set; }
        public decimal TotalEmAberto { get; set; }
    }
}

