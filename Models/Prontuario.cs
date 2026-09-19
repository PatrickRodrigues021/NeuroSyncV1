using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa a pasta clínica principal (prontuário) associada ao paciente no banco de dados SQLite.
/// Mapeada para a tabela 'prontuario'.
/// </summary>
[Table("prontuario")]
public class Prontuario
{
    [Key]
    [Column("id_prontuario")]
    public int Id { get; set; }

    /// <summary>
    /// Data de abertura do prontuário do paciente.
    /// </summary>
    [Required]
    [Column("data_criacao")]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>
    /// Chave estrangeira para o paciente atendido.
    /// </summary>
    [Required]
    [Column("id_paciente")]
    public int IdPaciente { get; set; }

    [ForeignKey("IdPaciente")]
    public Paciente? Paciente { get; set; }

    /// <summary>
    /// Queixa principal e histórico relatado pelos responsáveis ou encaminhadores.
    /// </summary>
    [Column("queixa")]
    public string Queixa { get; set; } = string.Empty;

    /// <summary>
    /// Vínculo com avaliação inicial (mantido para integridade da base legada).
    /// </summary>
    [Column("id_avaliacao")]
    public int? IdAvaliacao { get; set; }

    [ForeignKey("IdAvaliacao")]
    public Avaliacao? Avaliacao { get; set; }

    /// <summary>
    /// Vínculo com plano de intervenção (mantido para integridade da base legada).
    /// </summary>
    [Column("id_intervencao")]
    public int? IdIntervencao { get; set; }

    [ForeignKey("IdIntervencao")]
    public Intervencao? Intervencao { get; set; }
}