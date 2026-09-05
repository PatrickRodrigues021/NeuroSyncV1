using System;
using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Models
{
    public class Terapia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da terapia é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Descricao { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Icone { get; set; } = "bi-puzzle";

        [MaxLength(20)]
        public string CorHex { get; set; } = "#2563EB";

        [MaxLength(20)]
        public string CorFundoHex { get; set; } = "#EFF6FF";

        public int PacientesAtivosCount { get; set; }

        public int ProfissionaisCount { get; set; }

        public int SessoesMesCount { get; set; }
    }
}

