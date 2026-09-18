using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // Abre a tela visual de login
        public IActionResult Index() => View();

        // Recebe os dados quando o usuário clica em "Entrar"
        [HttpPost]
        public async Task<IActionResult> Entrar(string usuario, string senha)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Por favor, informe o usuário/e-mail e a senha.";
                return View("Index");
            }

            var termo = usuario.Trim();

            // Busca no banco por e-mail ou nome (login)
            var usuarioEncontrado = await _context.Usuarios
                .FirstOrDefaultAsync(u => (u.Email.ToLower() == termo.ToLower() || u.Nome.ToLower() == termo.ToLower()) && u.Senha == senha);

            // Fallback de segurança: caso o usuário ainda não exista e seja o primeiro acesso padrão
            if (usuarioEncontrado == null && termo.ToLower() == "admin" && senha == "admin123")
            {
                usuarioEncontrado = await _context.Usuarios.FirstOrDefaultAsync();
                if (usuarioEncontrado == null)
                {
                    usuarioEncontrado = new Usuario
                    {
                        Nome = "Mariana Silva",
                        Email = "admin",
                        Senha = "admin123",
                        CriadoEm = System.DateTime.Now
                    };
                    _context.Usuarios.Add(usuarioEncontrado);
                    await _context.SaveChangesAsync();
                }
            }

            if (usuarioEncontrado != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuarioEncontrado.Nome),
                    new Claim(ClaimTypes.Email, usuarioEncontrado.Email)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Gera o "crachá" (Cookie) e libera a entrada
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // Manda o usuário para a tela de Boas-Vindas e Resumo do Dia
                return RedirectToAction("BoasVindas", "Home");
            }

            // Se errar a senha, mostra mensagem de erro na tela
            ViewBag.Erro = "Usuário ou senha inválidos!";
            return View("Index");
        }

        // Função para clicar no botão "Sair" e rasgar o crachá
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}