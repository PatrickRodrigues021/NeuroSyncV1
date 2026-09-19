using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa um atendimento agendado na clínica.
/// Suporta modalidades como Avaliação Neuropsicopedagógica, Intervenção Cognitiva e Devolutiva.
/// </summary>
[Table("agendamento")]
public class Agendamento
{
    [Key]
    [Column("id_agendamento")]
    public int IdAgendamento { get; set; }

    [Required(ErrorMessage = "É obrigatório selecionar um paciente.")]
    [Column("id_paciente")]
    public int PacienteId { get; set; }

    [ForeignKey(nameof(PacienteId))]
    public Paciente? Paciente { get; set; }

    [Required(ErrorMessage = "A data e horário são obrigatórios.")]
    [Column("data_hora")]
    public DateTime DataHora { get; set; } = DateTime.Now;

    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Agendado"; // Agendado, Realizado, Cancelado, Falta, Em atendimento

    [MaxLength(50)]
    [Column("tipo_sessao")]
    public string TipoSessao { get; set; } = "Intervenção"; // Intervenção, Avaliação, Devolutiva, Orientação Escolar

    [MaxLength(500)]
    [Column("observacoes")]
    public string? Observacoes { get; set; }
}