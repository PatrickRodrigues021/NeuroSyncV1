using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador responsável pelas configurações da conta da profissional (perfil e segurança).
/// </summary>
[Authorize]
public class ConfiguracoesController(AppDbContext context) : Controller
{
    // =========================================================================
    // 1. MÉTODOS AUXILIARES
    // =========================================================================

    /// <summary>
    /// Localiza o usuário atualmente autenticado a partir dos claims da sessão ou fallback seguro.
    /// </summary>
    private async Task<Usuario?> ObterUsuarioAtualAsync()
    {
        // 1. Tenta buscar pelo ID numérico no Claim
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int idUsuario))
        {
            var userById = await context.Usuarios.FindAsync(idUsuario);
            if (userById != null) return userById;
        }

        // 2. Tenta buscar pelo E-mail no Claim
        var claimEmail = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrEmpty(claimEmail))
        {
            var userByEmail = await context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == claimEmail.ToLower());
            if (userByEmail != null) return userByEmail;
        }

        // 3. Tenta buscar pelo Nome de exibição
        var claimNome = User.Identity?.Name;
        if (!string.IsNullOrEmpty(claimNome))
        {
            var userByNome = await context.Usuarios.FirstOrDefaultAsync(u => u.Nome.ToLower() == claimNome.ToLower() || u.Email.ToLower() == claimNome.ToLower());
            if (userByNome != null) return userByNome;
        }

        return await context.Usuarios.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Extrai o primeiro nome ou título profissional para saudações (ex: "Dra. Mariana").
    /// </summary>
    public static string ExtrairPrimeiroNome(string? nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) return "Usuário";
        var partes = nomeCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "Usuário";

        string[] titulos = ["dr.", "dra.", "dr", "dra", "prof.", "profa.", "prof", "profa"];
        return titulos.Contains(partes[0].ToLower()) && partes.Length > 1
            ? $"{partes[0]} {partes[1]}"
            : partes[0];
    }

    // =========================================================================
    // 2. TELAS E AÇÕES DE CONFIGURAÇÃO
    // =========================================================================

    /// <summary>
    /// Exibe os dados cadastrais da profissional e o formulário de alteração de senha.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuario = await ObterUsuarioAtualAsync();
        if (usuario == null) return RedirectToAction("Index", "Login");

        return View(new ConfiguracoesViewModel
        {
            IdUsuario = usuario.IdUsuario,
            Nome = usuario.Nome,
            Email = usuario.Email,
            CriadoEm = usuario.CriadoEm,
            PrimeiroNome = ExtrairPrimeiroNome(usuario.Nome)
        });
    }

    /// <summary>
    /// Atualiza o nome e e-mail do usuário e renova a identidade de autenticação (Cookie).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarPerfil(AtualizarPerfilInputModel model)
    {
        var usuario = await ObterUsuarioAtualAsync();
        if (usuario == null) return RedirectToAction("Index", "Login");

        if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.Email))
        {
            TempData["ErroPerfil"] = "Nome e e-mail não podem ficar em branco.";
            return RedirectToAction(nameof(Index));
        }

        usuario.Nome = model.Nome.Trim();
        usuario.Email = model.Email.Trim();

        context.Update(usuario);
        await context.SaveChangesAsync();

        // Renova o Cookie de autenticação com o novo nome/e-mail
        Claim[] claims = [
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email)
        ];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        TempData["SucessoPerfil"] = "Dados do perfil atualizados com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Altera a senha de acesso da profissional com validação da senha atual.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaInputModel senhaModel)
    {
        var usuario = await ObterUsuarioAtualAsync();
        if (usuario == null) return RedirectToAction("Index", "Login");

        if (string.IsNullOrWhiteSpace(senhaModel.SenhaAtual))
        {
            TempData["ErroSenha"] = "A senha atual é obrigatória.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(senhaModel.NovaSenha) || senhaModel.NovaSenha.Length < 6)
        {
            TempData["ErroSenha"] = "A nova senha deve ter no mínimo 6 caracteres.";
            return RedirectToAction(nameof(Index));
        }

        if (senhaModel.NovaSenha != senhaModel.ConfirmarNovaSenha)
        {
            TempData["ErroSenha"] = "A confirmação de senha não confere com a nova senha.";
            return RedirectToAction(nameof(Index));
        }

        if (usuario.Senha != senhaModel.SenhaAtual)
        {
            TempData["ErroSenha"] = "A senha atual informada está incorreta.";
            return RedirectToAction(nameof(Index));
        }

        usuario.Senha = senhaModel.NovaSenha;
        context.Update(usuario);
        await context.SaveChangesAsync();

        TempData["SucessoSenha"] = "Sua senha foi alterada com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}
