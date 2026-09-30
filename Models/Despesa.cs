using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

[Table("despesa")]
public class Despesa
{
    [Key]
    [Column("id_despesa")]
    public int IdDespesa { get; set; }

    [Required(ErrorMessage = "A descrição da despesa é obrigatória.")]
    [MaxLength(150)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [MaxLength(50)]
    [Column("categoria")]
    public string Categoria { get; set; } = "Outros";

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Column("valor")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Pendente";

    [MaxLength(250)]
    [Column("observacoes")]
    public string? Observacoes { get; set; }
}
