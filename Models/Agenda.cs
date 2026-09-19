using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Entidade de agendamento de agenda legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'agenda' do banco de dados SQLite.
/// </summary>
[Table("agenda")]
public class Agenda
{
    [Key]
    [Column("id_agenda")]
    public int Id { get; set; }

    [Required]
    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Required]
    [Column("data_fim")]
    public DateTime DataFim { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Agendado";
}