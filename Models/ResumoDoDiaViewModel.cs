using System;
using System.Collections.Generic;

namespace NeuroSync.Models
{
    public class ResumoDoDiaViewModel
    {
        public string Saudacao { get; set; } = "Bom dia";
        public string NomeProfissional { get; set; } = "Dra. Mariana";
        public string DataFormatada { get; set; } = string.Empty;
        public string HoraFormatada { get; set; } = string.Empty;

        // Indicadores (Cards)
        public int TotalAtendimentosHoje { get; set; } = 6;
        public string DiferencaOntemTexto { get; set; } = "↑ 2 a mais que ontem";
        public int ConfirmadosCount { get; set; } = 5;
        public int ConfirmadosPercentual { get; set; } = 83;
        public int AguardandoCount { get; set; } = 1;
        public int AguardandoPercentual { get; set; } = 17;
        public int AniversariantesMesCount { get; set; } = 2;

        // Lista de Próximos Atendimentos
        public List<AtendimentoResumoItem> ProximosAtendimentos { get; set; } = new();
        public int MaisAtendimentosHojeCount { get; set; } = 3;

        // Lista de Aniversariantes do Mês
        public List<AniversarianteResumoItem> Aniversariantes { get; set; } = new();
    }

    public class AtendimentoResumoItem
    {
        public string Horario { get; set; } = string.Empty;
        public string NomePaciente { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty; // ex: "10 anos • Avaliação"
        public string Status { get; set; } = "Confirmado";
        public string GeneroOuAvatar { get; set; } = "boy"; // "boy", "girl"
    }

    public class AniversarianteResumoItem
    {
        public string NomePaciente { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty; // ex: "10 anos • 12 de setembro"
        public string GeneroOuAvatar { get; set; } = "boy";
    }
}

