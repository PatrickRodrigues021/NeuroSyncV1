using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Entidade de avaliação clínica da estrutura legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'avaliacao' do banco de dados SQLite.
/// </summary>
[Table("avaliacao")]
public class Avaliacao
{
    [Key]
    [Column("id_avaliacao")]
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
    /// Coleção de sessões vinculadas à avaliação.
    /// </summary>
    public ICollection<Sessao> Sessoes { get; set; } = [];
}