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
        [Column("id_prontuario")]
        public int ProntuarioId { get; set; }

        // Removido o Prontuario e o ForeignKey para evitar o erro de chave estrangeira

        [Required]
        [Column("titulo")]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("conteudo_criptografado")]
        public string ConteudoCriptografado { get; set; } = string.Empty;

        [Column("data_emissao")]
        public DateTime DataEmissao { get; set; } = DateTime.Now;
    }
}