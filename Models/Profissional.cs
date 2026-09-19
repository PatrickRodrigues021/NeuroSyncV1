namespace NeuroSync.Models;

/// <summary>
/// Entidade de profissional da saúde/educação legada.
/// Preservada para compatibilidade de integridade referencial com a tabela 'profissional' do banco de dados SQLite.
/// </summary>
public class Profissional
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Especialidade { get; set; }
}