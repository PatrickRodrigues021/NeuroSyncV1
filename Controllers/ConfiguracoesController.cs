using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NeuroSync.Data;
using NeuroSync.Models;

namespace NeuroSync.Controllers
{
    [Authorize]
    public class ConfiguracoesController : Controller
    {
        private readonly AppDbContext _context;

        public ConfiguracoesController(AppDbContext context)
        {
            _context = context;
        }

        // Helper para obter o usuário atualmente autenticado
        private async Task<Usuario?> ObterUsuarioAtualAsync()
        {
            // 1. Tenta buscar pelo ID registrado no Claim
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claimId, out int idUsuario))
            {
                var userById = await _context.Usuarios.FindAsync(idUsuario);
                if (userById != null) return userById;
            }

            // 2. Tenta buscar pelo Email do Claim
            var claimEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(claimEmail))
            {
                var userByEmail = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == claimEmail.ToLower());
                if (userByEmail != null) return userByEmail;
            }

            // 3. Tenta buscar pelo Nome do Claim
            var claimNome = User.Identity?.Name;
            if (!string.IsNullOrEmpty(claimNome))
            {
                var userByNome = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nome.ToLower() == claimNome.ToLower() || u.Email.ToLower() == claimNome.ToLower());
                if (userByNome != null) return userByNome;
            }

            // 4. Fallback para o primeiro usuário cadastrado
            return await _context.Usuarios.FirstOrDefaultAsync();
        }

        // Helper para extrair o primeiro nome de saudação
        public static string ExtrairPrimeiroNome(string? nomeCompleto)
        {
            if (string.IsNullOrWhiteSpace(nomeCompleto)) return "Usuário";
            var partes = nomeCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "Usuário";

            var titulos = new[] { "dr.", "dra.", "dr", "dra", "prof.", "profa.", "prof", "profa" };
            if (titulos.Contains(partes[0].ToLower()) && partes.Length > 1)
            {
                return $"{partes[0]} {partes[1]}";
            }
            return partes[0];
        }

        // GET: /Configuracoes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new ConfiguracoesViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CriadoEm = usuario.CriadoEm,
                PrimeiroNome = ExtrairPrimeiroNome(usuario.Nome)
            };

            return View(model);
        }

        // POST: /Configuracoes/AtualizarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarPerfil(AtualizarPerfilInputModel model)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                TempData["ErroPerfil"] = "O nome não pode ficar em branco.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                TempData["ErroPerfil"] = "O e-mail/usuário não pode ficar em branco.";
                return RedirectToAction("Index");
            }

            // Atualiza os dados no banco
            usuario.Nome = model.Nome.Trim();
            usuario.Email = model.Email.Trim();

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            // Atualiza o Cookie de sessão com os novos dados em tempo real
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SucessoPerfil"] = "Dados do perfil atualizados com sucesso!";
            return RedirectToAction("Index");
        }

        // POST: /Configuracoes/AlterarSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(AlterarSenhaInputModel senhaModel)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrWhiteSpace(senhaModel.SenhaAtual))
            {
                TempData["ErroSenha"] = "A senha atual é obrigatória.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(senhaModel.NovaSenha) || senhaModel.NovaSenha.Length < 6)
            {
                TempData["ErroSenha"] = "A nova senha deve ter no mínimo 6 caracteres.";
                return RedirectToAction("Index");
            }

            if (senhaModel.NovaSenha != senhaModel.ConfirmarNovaSenha)
            {
                TempData["ErroSenha"] = "A confirmação de senha não confere com a nova senha digitada.";
                return RedirectToAction("Index");
            }

            // Valida se a senha atual confere
            if (usuario.Senha != senhaModel.SenhaAtual)
            {
                TempData["ErroSenha"] = "A senha atual informada está incorreta.";
                return RedirectToAction("Index");
            }

            // Atualiza a senha no banco
            usuario.Senha = senhaModel.NovaSenha;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["SucessoSenha"] = "Sua senha foi alterada com sucesso! Utilize-a em seu próximo login.";
            return RedirectToAction("Index");
        }
    }
}
