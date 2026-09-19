using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Entidade de liquidação financeira legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'pagamento' do banco de dados SQLite.
/// </summary>
[Table("pagamento")]
public class Pagamento
{
    [Key]
    [Column("id_pagamento")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Pendente";

    [Required]
    [Column("data_pagamento")]
    public DateTime DataPagamento { get; set; } = DateTime.Now;

    [Required]
    [Column("valor", TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Column("id_sessao")]
    public int? IdSessao { get; set; }

    [ForeignKey("IdSessao")]
    public Sessao? Sessao { get; set; }

    [Column("id_avaliacao")]
    public int? IdAvaliacao { get; set; }

    [ForeignKey("IdAvaliacao")]
    public Avaliacao? Avaliacao { get; set; }
}