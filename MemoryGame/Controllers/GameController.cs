using Microsoft.AspNetCore.Mvc;
using MemoryGame.Repositories;

namespace MemoryGame.Controllers
{
    public partial class GameController : Controller
    {
        private readonly IGameRepository _games;
        private readonly ICardRepository _cards;
        private readonly ITileRepository _tiles;
        private readonly IMoveRepository _moves;

        public GameController(
            IGameRepository games,
            ICardRepository cards,
            ITileRepository tiles,
            IMoveRepository moves)
        {
            _games = games;
            _cards = cards;
            _tiles = tiles;
            _moves = moves;
        }

        // LOBBY
        [HttpGet]
        public async Task<IActionResult> Lobby()
        {
            var games = await _games.GetActiveGamesAsync();
            return View(games);
        }

        // CREATE (player becomes PlayerOrder=1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var gameId = await _games.CreateGameAsync(userId.Value);
            return RedirectToAction("Play", new { id = gameId });
        }

        // JOIN (becomes PlayerOrder=2, game -> InProgress)
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // PLAY (shows board; seeds cards/tiles on first visit)
        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
            var game = await _games.GetByIdAsync(id);
            if (game == null)
            {
                TempData["Error"] = "Game not found.";
                return RedirectToAction("Lobby");
            }

            // Seed cards and board if needed
            if (await _cards.CountAsync() == 0)
                await _cards.SeedDefaultPairsAsync();

            if (!await _tiles.AnyForGameAsync(id))
                await _tiles.CreateForGameAsync(id, 16); // 4x4 board

            var tiles = await _tiles.GetByGameAsync(id);

            // selection from session (two-click flow)
            var sel1Key = $"sel1_{id}";
            var sel1 = HttpContext.Session.GetInt32(sel1Key);

            ViewBag.GameId = id;
            ViewBag.Status = game.Status;
            ViewBag.Sel1 = sel1;

            // simple counters
            ViewBag.TotalTiles = tiles.Count;
            ViewBag.MatchedTiles = tiles.Count(t => t.IsMatched);

            return View(tiles);
        }

        // FLIP (handles two clicks -> records a Move; marks matched tiles)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flip(int id, int tileId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var sel1Key = $"sel1_{id}";
            var sel1 = HttpContext.Session.GetInt32(sel1Key);

            if (sel1 is null)
            {
                HttpContext.Session.SetInt32(sel1Key, tileId);
                return RedirectToAction("Play", new { id });
            }
            else
            {
                var first = await _tiles.GetAsync(sel1.Value);
                var second = await _tiles.GetAsync(tileId);
                if (first == null || second == null || first.GameID != id || second.GameID != id)
                {
                    HttpContext.Session.Remove(sel1Key);
                    TempData["Error"] = "Invalid selection.";
                    return RedirectToAction("Play", new { id });
                }

                // ✅ Correct match rule: compare Card.PairKey, not CardID
                var key1 = await _tiles.GetPairKeyAsync(first.TileID);
                var key2 = await _tiles.GetPairKeyAsync(second.TileID);
                bool isMatch = !string.IsNullOrEmpty(key1) && key1 == key2;

                if (isMatch)
                    await _tiles.SetMatchedAsync(first.TileID, second.TileID);

                await _moves.RecordAsync(id, userId.Value, first.TileID, second.TileID, isMatch);

                // For one render, reveal both picked tiles (UX)
                TempData["LastPair"] = $"{first.TileID},{second.TileID},{(isMatch ? 1 : 0)}";

                HttpContext.Session.Remove(sel1Key);
                return RedirectToAction("Play", new { id });
            }
        }
    }
}
