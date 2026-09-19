using Microsoft.EntityFrameworkCore;
using NeuroSync.Models;

namespace NeuroSync.Data;

/// <summary>
/// Contexto principal do Entity Framework Core para o sistema NeuroSync.
/// Gerencia as tabelas clínicas, financeiras, de usuários e do prontuário eletrônico no SQLite.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // --- Autenticação e Usuários ---
    public DbSet<Usuario> Usuarios { get; set; }

    // --- Pacientes e Prontuário Clínico ---
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Prontuario> Prontuarios { get; set; }
    public DbSet<Evolucao> Evolucoes { get; set; }
    public DbSet<ParecerTecnico> PareceresTecnicos { get; set; }
    public DbSet<Anexo> Anexos { get; set; }

    // --- Agenda e Atendimentos ---
    public DbSet<Agendamento> Agendamentos { get; set; }

    // --- Gestão Financeira (Receitas e Despesas) ---
    public DbSet<Cobranca> Cobrancas { get; set; }
    public DbSet<Despesa> Despesas { get; set; }

    // --- Entidades Legadas mantidas para integridade do banco SQLite ---
    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<Sessao> Sessoes { get; set; }
    public DbSet<Agenda> Agendas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Garante a conexão com o banco SQLite local neurosync.db
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=neurosync.db");
        }
    }
}