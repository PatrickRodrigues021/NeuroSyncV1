using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Entidade de plano de intervenção terapêutica legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'intervencao' do banco de dados SQLite.
/// </summary>
[Table("intervencao")]
public class Intervencao
{
    [Key]
    [Column("id_intervencao")]
    public int Id { get; set; }

    [Required]
    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Column("data_fim")]
    public DateTime? DataFim { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Em Andamento";

    /// <summary>
    /// Coleção de sessões vinculadas à intervenção.
    /// </summary>
    public ICollection<Sessao> Sessoes { get; set; } = [];
}