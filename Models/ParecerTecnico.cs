using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa o documento formal de Parecer Técnico ou Relatório de Avaliação Neuropsicopedagógica,
/// contendo os procedimentos, análise das funções cognitivas, síntese e recomendações para família e escola.
/// Mapeada para a tabela 'parecer_tecnico' do banco de dados SQLite.
/// </summary>
[Table("parecer_tecnico")]
public class ParecerTecnico
{
    [Key]
    [Column("id_parecer")]
    public int IdParecer { get; set; }

    /// <summary>
    /// Chave estrangeira para o paciente avaliado.
    /// </summary>
    [Required]
    [Column("id_paciente")]
    public int PacienteId { get; set; }

    [ForeignKey("PacienteId")]
    public Paciente? Paciente { get; set; }

    /// <summary>
    /// Título oficial do documento emitido.
    /// </summary>
    [Required(ErrorMessage = "O título do relatório é obrigatório.")]
    [MaxLength(200)]
    [Column("titulo")]
    public string Titulo { get; set; } = "Relatório de Avaliação Neuropsicopedagógica";

    /// <summary>
    /// Motivo da avaliação ou queixa principal inicial.
    /// </summary>
    [Column("motivo_avaliacao")]
    public string? MotivoAvaliacao { get; set; }

    /// <summary>
    /// Instrumentos de testagem, baterias cognitivas e procedimentos utilizados.
    /// </summary>
    [Column("procedimentos_recursos")]
    public string? ProcedimentosRecursos { get; set; }

    /// <summary>
    /// Análise qualitativa e quantitativa dos achados nas funções cognitivas avaliadas.
    /// </summary>
    [Column("analise_avaliativa")]
    public string? AnaliseAvaliativa { get; set; }

    /// <summary>
    /// Conclusão e síntese diagnóstica/interventiva.
    /// </summary>
    [Column("sintese_avaliativa")]
    public string? SinteseAvaliativa { get; set; }

    /// <summary>
    /// Orientações e encaminhamentos para a escola e familiares.
    /// </summary>
    [Column("recomendacoes_finais")]
    public string? RecomendacoesFinais { get; set; }

    /// <summary>
    /// Data de emissão do parecer técnico.
    /// </summary>
    [Column("data_emissao")]
    public DateTime DataEmissao { get; set; } = DateTime.Now;

    /// <summary>
    /// Nome da neuropsicopedagoga emissora.
    /// </summary>
    [MaxLength(100)]
    [Column("profissional_nome")]
    public string ProfissionalNome { get; set; } = "Dra. Mariana Silva";

    /// <summary>
    /// Número do registro profissional (ex.: ABNp).
    /// </summary>
    [MaxLength(50)]
    [Column("registro_profissional")]
    public string? RegistroProfissional { get; set; } = "ABNp 1420/SP";
}
