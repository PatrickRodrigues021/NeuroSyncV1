using Microsoft.EntityFrameworkCore;
using NeuroSync.Models;

namespace NeuroSync.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Prontuario> Prontuarios { get; set; }
    public DbSet<Evolucao> Evolucoes { get; set; }
    public DbSet<ParecerTecnico> PareceresTecnicos { get; set; }
    public DbSet<Anexo> Anexos { get; set; }

    public DbSet<Agendamento> Agendamentos { get; set; }

    public DbSet<Cobranca> Cobrancas { get; set; }
    public DbSet<Despesa> Despesas { get; set; }

    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<Sessao> Sessoes { get; set; }
    public DbSet<Agenda> Agendas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=neurosync.db");
        }
    }
}