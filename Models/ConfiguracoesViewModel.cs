using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Models;

/// <summary>
/// Modelo de exibição para a tela de Configurações de Perfil e Segurança.
/// </summary>
public class ConfiguracoesViewModel
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public string PrimeiroNome { get; set; } = string.Empty;
}

/// <summary>
/// Modelo de formulário para edição dos dados cadastrais do perfil da profissional.
/// </summary>
public class AtualizarPerfilInputModel
{
    public int IdUsuario { get; set; }

    [Display(Name = "Nome Completo")]
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "E-mail ou Usuário de Acesso")]
    [Required(ErrorMessage = "O e-mail/usuário é obrigatório.")]
    [StringLength(150, ErrorMessage = "O e-mail/usuário deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }

    public string PrimeiroNome { get; set; } = string.Empty;

    /// <summary>
    /// Sub-modelo para alteração opcional de senha de acesso.
    /// </summary>
    public AlterarSenhaInputModel SenhaModel { get; set; } = new();
}

/// <summary>
/// Modelo de validação para alteração segura de senha com verificação de requisitos mínimos.
/// </summary>
public class AlterarSenhaInputModel
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha Atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Nova Senha")]
    [Compare("NovaSenha", ErrorMessage = "A confirmação de senha não confere com a nova senha digitada.")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}
