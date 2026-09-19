using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa uma cobrança de receita clínica (sessões avulsas, pacotes ou mensalidades).
/// Mapeada para a tabela 'cobranca' do banco de dados SQLite.
/// </summary>
[Table("cobranca")]
public class Cobranca
{
    [Key]
    [Column("id_cobranca")]
    public int IdCobranca { get; set; }

    /// <summary>
    /// Chave estrangeira para o paciente cobrado (obrigatório).
    /// </summary>
    [Required(ErrorMessage = "O paciente é obrigatório.")]
    [Column("id_paciente")]
    public int PacienteId { get; set; }

    [ForeignKey("PacienteId")]
    public Paciente? Paciente { get; set; }

    /// <summary>
    /// Vínculo opcional com um agendamento específico (para cobranças avulsas).
    /// </summary>
    [Column("id_agendamento")]
    public int? AgendamentoId { get; set; }

    [ForeignKey("AgendamentoId")]
    public Agendamento? Agendamento { get; set; }

    /// <summary>
    /// Valor monetário da cobrança.
    /// </summary>
    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Data limite para pagamento.
    /// </summary>
    [Required]
    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    /// <summary>
    /// Data efetiva de liquidação financeira (preenchida na baixa).
    /// </summary>
    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    /// <summary>
    /// Estado da cobrança ("Pendente", "Pago", "Atrasado", "Cancelado").
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Pendente";

    /// <summary>
    /// Descrição resumida da receita (ex.: "Sessão de Intervenção", "Mensalidade Setembro").
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;
}