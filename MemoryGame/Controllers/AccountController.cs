using Microsoft.AspNetCore.Mvc;
using MemoryGame.Repositories;

namespace MemoryGame.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _users;
        public AccountController(IUserRepository users) => _users = users;

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and Password are required.");
                return View();
            }

            var existing = await _users.GetByUsernameAsync(username);
            if (existing != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View();
            }

            var id = await _users.CreateAsync(username, email ?? "", password);
            HttpContext.Session.SetInt32("UserID", id);
            HttpContext.Session.SetString("Username", username);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _users.ValidateAsync(username, password);
            if (user is null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Content("AccountController is alive. Try /Account/Register or /Account/Login.");
        }
    }
}
