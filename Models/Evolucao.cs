using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa o registro clínico de evolução diária ou de sessão no prontuário do paciente.
/// Mapeada para a tabela 'evolucao' do banco de dados SQLite.
/// </summary>
[Table("evolucao")]
public class Evolucao
{
    [Key]
    [Column("id_evolucao")]
    public int IdEvolucao { get; set; }

    /// <summary>
    /// Chave estrangeira do paciente associado.
    /// </summary>
    [Required]
    [Column("id_paciente")]
    public int PacienteId { get; set; }
    
    [ForeignKey("PacienteId")]
    public Paciente? Paciente { get; set; }

    /// <summary>
    /// Data e hora do registro da evolução clínica.
    /// </summary>
    [Required]
    [Column("data_registro")]
    public DateTime DataRegistro { get; set; } = DateTime.Now;

    /// <summary>
    /// Texto descritivo das observações, intervenções e respostas comportamentais/cognitivas do paciente.
    /// </summary>
    [Required(ErrorMessage = "A anotação da evolução é obrigatória.")]
    [Column("anotacao", TypeName = "text")]
    public string Anotacao { get; set; } = string.Empty;

    /// <summary>
    /// Categoria da anotação clínica (ex.: "Sessão Terapêutica", "Intervenção Cognitiva", "Avaliação").
    /// </summary>
    [MaxLength(100)]
    [Column("tipo_evolucao")]
    public string TipoEvolucao { get; set; } = "Sessão Terapêutica";

    /// <summary>
    /// Nome da profissional responsável pelo atendimento registrado.
    /// </summary>
    [MaxLength(100)]
    [Column("profissional_nome")]
    public string? ProfissionalNome { get; set; }
}