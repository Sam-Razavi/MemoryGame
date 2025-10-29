using Microsoft.AspNetCore.Mvc;
using MemoryGame.Repositories;

namespace MemoryGame.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameRepository _games;
        public GameController(IGameRepository games) => _games = games;

        [HttpGet]
        public async Task<IActionResult> Lobby()
        {
            var games = await _games.GetActiveGamesAsync();
            return View(games);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var gameId = await _games.CreateGameAsync(userId.Value);
            return RedirectToAction("Play", new { id = gameId });
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var ok = await _games.JoinGameAsync(id, userId.Value);
            if (!ok)
            {
                TempData["Error"] = "Game is full or already started.";
                return RedirectToAction("Lobby");
            }

            return RedirectToAction("Play", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
            var game = await _games.GetByIdAsync(id);
            if (game == null)
            {
                TempData["Error"] = "Game not found.";
                return RedirectToAction("Lobby");
            }

            ViewBag.GameId = game.GameID;
            ViewBag.Status = game.Status;
            return View();
        }
    }
}
