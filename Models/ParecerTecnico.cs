using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models
{
    [Table("parecer_tecnico")]
    public class ParecerTecnico
    {
        [Key]
        [Column("id_parecer")]
        public int IdParecer { get; set; }

        [Required]
        [Column("id_paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Paciente? Paciente { get; set; }

        [Required(ErrorMessage = "O título do relatório é obrigatório.")]
        [MaxLength(200)]
        [Column("titulo")]
        public string Titulo { get; set; } = "Relatório de Avaliação Neuropsicopedagógica";

        [Column("motivo_avaliacao")]
        public string? MotivoAvaliacao { get; set; }

        [Column("procedimentos_recursos")]
        public string? ProcedimentosRecursos { get; set; }

        [Column("analise_avaliativa")]
        public string? AnaliseAvaliativa { get; set; }

        [Column("sintese_avaliativa")]
        public string? SinteseAvaliativa { get; set; }

        [Column("recomendacoes_finais")]
        public string? RecomendacoesFinais { get; set; }

        [Column("data_emissao")]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        [MaxLength(100)]
        [Column("profissional_nome")]
        public string ProfissionalNome { get; set; } = "Dra. Mariana Silva";

        [MaxLength(50)]
        [Column("registro_profissional")]
        public string? RegistroProfissional { get; set; } = "ABNp 1420/SP";
    }
}

