using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Eco_life.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Eco_life.Pages
{
    public class LoginFuncionarioModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginFuncionarioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email_Funcionario { get; set; }

        [BindProperty]
        public string? Senha_Funcionario { get; set; }

        public IActionResult OnPost()
        {
            var funcionario = _context.Funcionarios1
                .FirstOrDefault(f => f.Email_Funcionario == Email_Funcionario && f.Senha_Funcionario == Senha_Funcionario);

            if (funcionario != null)
            {
                // Autenticação bem-sucedida
                HttpContext.Session.SetString("FuncionarioId", funcionario.Id_Funcionario.ToString());
                HttpContext.Session.SetString("FuncionarioNome", funcionario.Nome_Funcionario ?? "");
                HttpContext.Session.SetString("FuncionarioEmail", funcionario.Email_Funcionario ?? "");
                return RedirectToPage("/PerfilFuncionario");
            }

            // Falha na autenticação
            ModelState.AddModelError("", "Email ou senha inválidos.");
            return Page();
        }
    }
}
