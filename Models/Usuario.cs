using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroSync.Models;

/// <summary>
/// Representa a usuária do sistema com credenciais de autenticação.
/// Mapeada para a tabela 'usuario' do banco de dados SQLite.
/// </summary>
[Table("usuario")]
public class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nome completo da usuária para saudações e assinaturas em relatórios.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// E-mail de login no sistema.
    /// </summary>
    [Required]
    [MaxLength(150)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash ou texto de segurança da senha de acesso.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("senha")]
    public string Senha { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do registro de usuário.
    /// </summary>
    [Required]
    [Column("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}