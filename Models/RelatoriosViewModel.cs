using System;
using System.Collections.Generic;

namespace NeuroSync.Models
{
    public class RelatoriosViewModel
    {
        // Filtros e Navegação
        public string AbaAtiva { get; set; } = "Indicadores";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string PeriodoTexto { get; set; } = string.Empty;
        public string Periodicidade { get; set; } = "Mensal";

        // KPI 1: Atendimentos Realizados
        public int AtendimentosRealizados { get; set; }
        public double VariacaoAtendimentos { get; set; }

        // KPI 2: Novos Pacientes
        public int NovosPacientes { get; set; }
        public double VariacaoNovosPacientes { get; set; }

        // KPI 3: Taxa de Faltas
        public double TaxaFaltas { get; set; }
        public double VariacaoTaxaFaltas { get; set; }

        // KPI 4: Satisfação Média
        public double SatisfacaoMedia { get; set; }
        public double VariacaoSatisfacao { get; set; }

        // Gráfico 1: Atendimentos por Área
        public List<AtendimentoAreaItem> AtendimentosPorArea { get; set; } = new();

        // Gráfico 2: Evolução de Atendimentos
        public List<string> EvolucaoLabels { get; set; } = new();
        public List<int> EvolucaoValores { get; set; } = new();
    }

    public class AtendimentoAreaItem
    {
        public string NomeArea { get; set; } = string.Empty;
        public int Porcentagem { get; set; }
        public int Quantidade { get; set; }
        public string CorHex { get; set; } = "#315BEF";
    }
}

