using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NeuroSync.Models
{
    public class FinanceiroViewModel
    {
        public List<Cobranca> Cobrancas { get; set; } = new();
        
        // Totais e Receitas
        public decimal ReceitaMes { get; set; }
        public decimal TotalRecebido { get; set; }
        public decimal TotalAReceber { get; set; }
        public decimal TotalEmAberto { get; set; }
        
        // Despesas e Variações
        public decimal TotalDespesas { get; set; }
        public decimal DespesasMes { get; set; }
        public string VariacaoDespesas { get; set; } = "+0%";
        public string VariacaoReceitaMes { get; set; } = "+0%";
        
        // Indicadores e Percentuais
        public decimal TaxaInadimplencia { get; set; }
        public decimal PercentualRecebido { get; set; }
        public decimal PercentualAReceber { get; set; }
        
        // Filtros e Abas da View
        public int? PacienteId { get; set; }
        public string? PacienteNome { get; set; }
        public string CompetenciaSelecionada { get; set; } = "";
        public string AbaAtiva { get; set; } = "geral";
        public string StatusSelecionado { get; set; } = "";
        
        // Listas para Selects / Filtros
        public SelectList? ListaPacientes { get; set; }
        public SelectList? ListaStatus { get; set; }
        
        // Gráficos
        public List<string> MesesLabels { get; set; } = new() { "Mai", "Jun", "Jul", "Ago", "Set" };
        public List<decimal> ReceitasMensais { get; set; } = new() { 300, 250, 400, 350, 0 };
        public List<decimal> DespesasMensais { get; set; } = new() { 150, 120, 180, 140, 0 };
    }
}