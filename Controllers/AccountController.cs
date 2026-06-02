using Microsoft.AspNetCore.Mvc;
using SiteNamorada.Data;
using SiteNamorada.Models;

namespace SiteNamorada.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Cadastro()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Cadastro(CadastroViewModel model)
    {
        var usuario = new Usuario
        {
            Nome = model.Nome,
            Email = model.Email,
            Senha = model.Senha
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        var usuario = _context.Usuarios.FirstOrDefault(x =>
            x.Email == model.Email &&
            x.Senha == model.Senha);

        if (usuario == null)
            return View();

        HttpContext.Session.SetString("Usuario", usuario.Nome);

        return RedirectToAction("Painel", "Home");
    }
}