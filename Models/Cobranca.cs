using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

[Table("cobranca")]
public class Cobranca
{
    [Key]
    [Column("id_cobranca")]
    public int IdCobranca { get; set; }

    [Required(ErrorMessage = "O paciente é obrigatório.")]
    [Column("id_paciente")]
    public int PacienteId { get; set; }

    [ForeignKey("PacienteId")]
    public Paciente? Paciente { get; set; }

    [Column("id_agendamento")]
    public int? AgendamentoId { get; set; }

    [ForeignKey("AgendamentoId")]
    public Agendamento? Agendamento { get; set; }

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [Required]
    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Pendente";

    [Required]
    [MaxLength(100)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;
}