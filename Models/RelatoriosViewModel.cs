namespace NeuroSync.Models;

/// <summary>
/// Modelo unificado de exibição para a tela de Relatórios e Indicadores Clínicos (Abas Indicadores e Atendimentos).
/// </summary>
public class RelatoriosViewModel
{
    /// <summary>
    /// Aba ativa da tela ("Indicadores" ou "Atendimentos").
    /// </summary>
    public string AbaAtiva { get; set; } = "Indicadores";
    
    // Filtros de Período
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string PeriodoTexto { get; set; } = string.Empty;

    // Filtros Operacionais da Aba Atendimentos
    public int? PacienteId { get; set; }
    public string TipoSessaoSelecionado { get; set; } = "Todos";
    public string StatusSelecionado { get; set; } = "Todos";

    // ==========================================
    // KPIs CLÍNICOS DA NEUROPSICOPEDAGOGA
    // ==========================================
    public int AtendimentosRealizados { get; set; }
    public double VariacaoAtendimentos { get; set; }

    public double TaxaAssiduidade { get; set; }
    public double VariacaoAssiduidade { get; set; }

    public int PacientesAtivosCount { get; set; }
    public double VariacaoPacientes { get; set; }

    public int NovasAvaliacoesCount { get; set; }
    public double VariacaoNovasAvaliacoes { get; set; }

    // Indicadores de Prontuário e Pareceres
    public int EvolucoesRegistradasCount { get; set; }
    public int PareceresEmitidosCount { get; set; }

    // ==========================================
    // GRÁFICOS CLÍNICOS
    // ==========================================
    // Gráfico 1: Evolução Temporal de Atendimentos (Últimos 5 meses)
    public List<string> EvolucaoLabels { get; set; } = [];
    public List<int> EvolucaoValores { get; set; } = [];

    // Gráfico 2: Foco Clínico Neuropsicopedagógico (Donut Chart)
    public List<FocoClinicoItem> FocoClinicoItens { get; set; } = [];

    // ==========================================
    // LISTAGEM ANALÍTICA DE ATENDIMENTOS
    // ==========================================
    public List<AgendamentoRelatorioItem> Atendimentos { get; set; } = [];
    public int TotalSessoesPeriodo { get; set; }
    public int TotalFaltasPeriodo { get; set; }
    public int TotalCanceladosPeriodo { get; set; }
}

/// <summary>
/// Segmento de foco clínico para o gráfico donut de distribuição do tempo de atendimento.
/// </summary>
public class FocoClinicoItem
{
    public string NomeFoco { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int Porcentagem { get; set; }
    public string CorHex { get; set; } = "#2563EB";
    public string Icone { get; set; } = "bi-puzzle";
}

/// <summary>
/// Linha analítica de atendimento com verificação de prontuário na listagem de relatórios.
/// </summary>
public class AgendamentoRelatorioItem
{
    public int IdAgendamento { get; set; }
    public DateTime DataHora { get; set; }
    public int PacienteId { get; set; }
    public string PacienteNome { get; set; } = string.Empty;
    public string? PacienteTelefone { get; set; }
    public string TipoSessao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public bool TemEvolucaoRegistrada { get; set; }
}
