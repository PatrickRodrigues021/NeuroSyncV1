using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Entidade de atendimento de sessão legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'sessao' do banco de dados SQLite.
/// </summary>
[Table("sessao")]
public class Sessao
{
    [Key]
    [Column("id_sessao")]
    public int Id { get; set; }

    [Required]
    [Column("data")]
    public DateTime Data { get; set; }

    [Column("observacoes")]
    public string Observacoes { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("anexos")]
    public string Anexos { get; set; } = string.Empty;

    [Required]
    [Column("id_avaliacao")]
    public int IdAvaliacao { get; set; }

    [ForeignKey("IdAvaliacao")]
    public Avaliacao? Avaliacao { get; set; }

    [Column("id_intervencao")]
    public int? IdIntervencao { get; set; }

    [ForeignKey("IdIntervencao")]
    public Intervencao? Intervencoes { get; set; }
}