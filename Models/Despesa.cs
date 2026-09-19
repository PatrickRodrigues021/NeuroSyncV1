using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa uma conta ou custo operacional do consultório (aluguel, energia, materiais, sistemas, etc.).
/// Mapeada para a tabela 'despesa' do banco de dados SQLite.
/// </summary>
[Table("despesa")]
public class Despesa
{
    [Key]
    [Column("id_despesa")]
    public int IdDespesa { get; set; }

    /// <summary>
    /// Descrição detalhada do custo (ex.: "Conta de Luz - CEMIG", "Assinatura do Software").
    /// </summary>
    [Required(ErrorMessage = "A descrição da despesa é obrigatória.")]
    [MaxLength(150)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Categoria da despesa (Energia Elétrica, Água, Internet, Telefonia, Material de Escritório, Sistemas, Aluguel, Limpeza, Outros).
    /// </summary>
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [MaxLength(50)]
    [Column("categoria")]
    public string Categoria { get; set; } = "Outros";

    /// <summary>
    /// Valor financeiro da despesa.
    /// </summary>
    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Column("valor")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Data de vencimento da fatura.
    /// </summary>
    [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    /// <summary>
    /// Data em que a despesa foi quitada (nulo se ainda pendente).
    /// </summary>
    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    /// <summary>
    /// Status do pagamento ("Pago", "Pendente", "Atrasado").
    /// </summary>
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Pendente";

    /// <summary>
    /// Anotações complementares ou número de recibo/comprovante.
    /// </summary>
    [MaxLength(250)]
    [Column("observacoes")]
    public string? Observacoes { get; set; }
}
