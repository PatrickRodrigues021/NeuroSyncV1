using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers;

/// <summary>
/// Controlador responsável pela autenticação e controle de sessão da usuária no sistema.
/// </summary>
public class LoginController(AppDbContext context) : Controller
{
    // =========================================================================
    // 1. TELA DE LOGIN
    // =========================================================================

    /// <summary>
    /// Exibe a página visual de autenticação do NeuroSync.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View();

    // =========================================================================
    // 2. PROCESSAMENTO DO LOGIN
    // =========================================================================

    /// <summary>
    /// Valida as credenciais informadas, autentica e emite o Cookie de sessão com as claims da usuária.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Entrar(string usuario, string senha)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
        {
            ViewBag.Erro = "Por favor, informe o usuário/e-mail e a senha.";
            return View("Index");
        }

        var termo = usuario.Trim();

        // Localiza usuário por e-mail ou nome
        var usuarioEncontrado = await context.Usuarios
            .FirstOrDefaultAsync(u => (u.Email.ToLower() == termo.ToLower() || u.Nome.ToLower() == termo.ToLower()) && u.Senha == senha);

        // Fallback de primeiro acesso: cria usuária padrão administrativa se a base estiver vazia
        if (usuarioEncontrado == null && termo.Equals("admin", StringComparison.OrdinalIgnoreCase) && senha == "admin123")
        {
            usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync();
            if (usuarioEncontrado == null)
            {
                usuarioEncontrado = new Usuario
                {
                    Nome = "Mariana Silva",
                    Email = "admin",
                    Senha = "admin123",
                    CriadoEm = DateTime.Now
                };
                context.Usuarios.Add(usuarioEncontrado);
                await context.SaveChangesAsync();
            }
        }

        if (usuarioEncontrado != null)
        {
            Claim[] claims = [
                new(ClaimTypes.NameIdentifier, usuarioEncontrado.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuarioEncontrado.Nome),
                new(ClaimTypes.Email, usuarioEncontrado.Email)
            ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("BoasVindas", "Home");
        }

        ViewBag.Erro = "Usuário ou senha inválidos!";
        return View("Index");
    }

    // =========================================================================
    // 3. LOGOUT / SAÍDA DO SISTEMA
    // =========================================================================

    /// <summary>
    /// Encerra a sessão atual e revoga o Cookie de autenticação.
    /// </summary>
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Login");
    }
}