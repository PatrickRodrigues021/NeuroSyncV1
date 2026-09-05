using System.Collections.Generic;

namespace NeuroSync.Models
{
    public class TerapiaDetalhesViewModel
    {
        public Terapia Terapia { get; set; } = new();
        public List<Profissional> Profissionais { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new();
        public List<Agendamento> ProximosAgendamentos { get; set; } = new();
    }
}

