using System;
using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Models
{
    public class ConfiguracoesViewModel
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public string PrimeiroNome { get; set; } = string.Empty;
    }

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

        // Model para alteração de senha
        public AlterarSenhaInputModel SenhaModel { get; set; } = new AlterarSenhaInputModel();
    }

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
}

