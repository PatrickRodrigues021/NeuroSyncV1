namespace NeuroSync.Models;

public class RelatoriosViewModel
{
    public string AbaAtiva { get; set; } = "Indicadores";
    
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string PeriodoTexto { get; set; } = string.Empty;

    public int? PacienteId { get; set; }
    public string TipoSessaoSelecionado { get; set; } = "Todos";
    public string StatusSelecionado { get; set; } = "Todos";

    public int AtendimentosRealizados { get; set; }
    public double VariacaoAtendimentos { get; set; }

    public double TaxaAssiduidade { get; set; }
    public double VariacaoAssiduidade { get; set; }

    public int PacientesAtivosCount { get; set; }
    public double VariacaoPacientes { get; set; }

    public int NovasAvaliacoesCount { get; set; }
    public double VariacaoNovasAvaliacoes { get; set; }

    public int EvolucoesRegistradasCount { get; set; }
    public int PareceresEmitidosCount { get; set; }

    public List<string> EvolucaoLabels { get; set; } = [];
    public List<int> EvolucaoValores { get; set; } = [];

    public List<FocoClinicoItem> FocoClinicoItens { get; set; } = [];

    public List<AgendamentoRelatorioItem> Atendimentos { get; set; } = [];
    public int TotalSessoesPeriodo { get; set; }
    public int TotalFaltasPeriodo { get; set; }
    public int TotalCanceladosPeriodo { get; set; }
}

public class FocoClinicoItem
{
    public string NomeFoco { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int Porcentagem { get; set; }
    public string CorHex { get; set; } = "#2563EB";
    public string Icone { get; set; } = "bi-puzzle";
}
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
