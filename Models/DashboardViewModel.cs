using System;
using System.Collections.Generic;

namespace NeuroSync.Models
{
    public class DashboardViewModel
    {
        // Cabeçalho
        public string Saudacao { get; set; } = "Bom dia";
        public string NomeUsuario { get; set; } = "Dra. Mariana";
        public string DataFormatada { get; set; } = string.Empty;

        // 4 Cards Clínicos Principais
        public int AtendimentosHoje { get; set; } = 8;
        public int ConcluidosHoje { get; set; } = 3;
        public int ConfirmadosHoje { get; set; } = 6;
        public int ConfirmadosPercentual { get; set; } = 75;
        public int AguardandoConfirmacaoHoje { get; set; } = 2;
        public int AguardandoPercentual { get; set; } = 25;
        public int PendenciasHoje { get; set; } = 5;

        // Métricas Secundárias (Faixa Horizontal)
        public int TotalPacientesAtivos { get; set; } = 34;
        public decimal ReceitaMes { get; set; } = 12500m;
        public int SessoesRealizadasMes { get; set; } = 42;
        public int AniversariantesMes { get; set; } = 2;

        // Próximos Atendimentos
        public List<DashboardAtendimentoItem> ProximosAtendimentos { get; set; } = new();

        // Status das Sessões (Mês)
        public string NomeMes { get; set; } = "Setembro";
        public int TotalSessoesMes { get; set; } = 13;
        public int RealizadasMes { get; set; } = 2;
        public int AgendadasMes { get; set; } = 10;
        public int CanceladasMes { get; set; } = 1;
        public int FaltasMes { get; set; } = 0;

        // Ritmo Semanal (Distribuição Seg a Sex)
        public int SemanaSeg { get; set; } = 3;
        public int SemanaTer { get; set; } = 5;
        public int SemanaQua { get; set; } = 4;
        public int SemanaQui { get; set; } = 7;
        public int SemanaSex { get; set; } = 6;
    }

    public class DashboardAtendimentoItem
    {
        public string Horario { get; set; } = string.Empty;
        public string PacienteNome { get; set; } = string.Empty;
        public string Terapia { get; set; } = "Fonoaudiologia";
        public string Sala { get; set; } = "Sala 02";
        public string Status { get; set; } = "Confirmado";
        public string AvatarGenero { get; set; } = "boy"; // "boy", "girl"
        public int PacienteId { get; set; }
        public int AgendamentoId { get; set; }
    }
}

