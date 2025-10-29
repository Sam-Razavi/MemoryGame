using Microsoft.AspNetCore.Mvc;
using MemoryGame.Repositories;
using MemoryGame.Models;
using System.Linq;

namespace MemoryGame.Controllers
{
    public class GameController : Controller
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

        // JOIN (PlayerOrder=2, game -> InProgress)
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

        // helper: determine whose turn it is
        private async Task<int?> GetCurrentTurnUserIdAsync(int gameId, List<PlayerVm> players)
        {
            var last = await _moves.GetLastMoveAsync(gameId);
            if (last is null)
            {
                return players.OrderBy(p => p.PlayerOrder).FirstOrDefault()?.UserID;
            }
            if (last.IsMatch) return last.UserID;
            var other = players.FirstOrDefault(p => p.UserID != last.UserID);
            return other?.UserID;
        }

        // PLAY
        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
            var game = await _games.GetByIdAsync(id);
            if (game == null)
            {
                TempData["Error"] = "Game not found.";
                return RedirectToAction("Lobby");
            }

            if (await _cards.CountAsync() == 0) await _cards.SeedDefaultPairsAsync();
            if (!await _tiles.AnyForGameAsync(id)) await _tiles.CreateForGameAsync(id, 16);

            var tiles = await _tiles.GetByGameAsync(id);

            var sel1Key = $"sel1_{id}";
            var sel1 = HttpContext.Session.GetInt32(sel1Key);

            ViewBag.GameId = id;
            ViewBag.Status = game.Status;
            ViewBag.Sel1 = sel1;

            var players = await _games.GetPlayersAsync(id);
            var scores = await _moves.GetScoresAsync(id);
            ViewBag.Players = players;
            ViewBag.Scores = scores;

            int totalTiles = tiles.Count;
            int matchedTiles = tiles.Count(t => t.IsMatched);
            int remainingPairs = (totalTiles - matchedTiles) / 2;

            ViewBag.TotalTiles = totalTiles;
            ViewBag.MatchedTiles = matchedTiles;
            ViewBag.RemainingPairs = remainingPairs;

            if (remainingPairs == 0 && game.Status != "Completed")
            {
                int score1 = players.Count > 0 && scores.ContainsKey(players[0].UserID) ? scores[players[0].UserID] : 0;
                int score2 = players.Count > 1 && scores.ContainsKey(players[1].UserID) ? scores[players[1].UserID] : 0;

                int? winnerGpid = null;
                if (score1 > score2) winnerGpid = players[0].GamePlayerID;
                else if (score2 > score1) winnerGpid = players[1].GamePlayerID;

                await _games.CompleteGameAsync(id, winnerGpid);
                game = await _games.GetByIdAsync(id);
            }

            int? currentTurnUserId = null;
            if (game.Status == "InProgress" || game.Status == "Waiting")
                currentTurnUserId = await GetCurrentTurnUserIdAsync(id, players);

            var me = HttpContext.Session.GetInt32("UserID");
            ViewBag.MeUserId = me;
            ViewBag.CurrentTurnUserId = currentTurnUserId;
            ViewBag.IsMyTurn = (me.HasValue && currentTurnUserId.HasValue && me.Value == currentTurnUserId.Value);
            ViewBag.GameCompleted = (game.Status == "Completed");

            if ((players?.Count ?? 0) == 1 && me.HasValue && me.Value == players[0].UserID && !((bool)ViewBag.GameCompleted))
            {
                ViewBag.IsMyTurn = true;
            }

            if (game.Status == "Completed")
            {
                string winnerName = "Tie";
                if (game.WinnerGamePlayerID.HasValue)
                {
                    var winner = players.FirstOrDefault(p => p.GamePlayerID == game.WinnerGamePlayerID.Value);
                    if (winner != null) winnerName = winner.Username;
                }
                ViewBag.WinnerName = winnerName;
            }

            var last = await _moves.GetLastMoveAsync(id);
            ViewBag.LastTurn = last?.TurnNumber ?? 0;

            var lastPair = TempData["LastPair"];
            if (lastPair != null) TempData["LastPair"] = lastPair;

            return View(tiles);
        }

        // FLIP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flip(int id, int tileId)
        {
            var me = HttpContext.Session.GetInt32("UserID");
            if (me is null) return RedirectToAction("Login", "Account");

            var game = await _games.GetByIdAsync(id);
            if (game == null) { TempData["Error"] = "Game not found."; return RedirectToAction("Lobby"); }
            if (game.Status == "Completed") { TempData["Error"] = "Game already completed."; return RedirectToAction("Play", new { id }); }

            var players = await _games.GetPlayersAsync(id);
            if (players.Count < 1) { TempData["Error"] = "No players in this game."; return RedirectToAction("Play", new { id }); }

            var currentTurnUserId = await GetCurrentTurnUserIdAsync(id, players);
            if (currentTurnUserId.HasValue && me.Value != currentTurnUserId.Value)
            {
                TempData["Error"] = "Not your turn.";
                return RedirectToAction("Play", new { id });
            }

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

                var key1 = await _tiles.GetPairKeyAsync(first.TileID);
                var key2 = await _tiles.GetPairKeyAsync(second.TileID);
                bool isMatch = !string.IsNullOrEmpty(key1) && key1 == key2;

                if (isMatch)
                    await _tiles.SetMatchedAsync(first.TileID, second.TileID);

                await _moves.RecordAsync(id, me.Value, first.TileID, second.TileID, isMatch);

                TempData["LastPair"] = $"{first.TileID},{second.TileID},{(isMatch ? 1 : 0)}";
                HttpContext.Session.Remove(sel1Key);

                return RedirectToAction("Play", new { id });
            }
        }

        // POLLING JSON
        [HttpGet]
        public async Task<IActionResult> State(int id)
        {
            var game = await _games.GetByIdAsync(id);
            if (game == null) return Json(new { ok = false });

            var players = await _games.GetPlayersAsync(id);
            int? currentTurnUserId = null;
            if (game.Status == "InProgress" || game.Status == "Waiting")
                currentTurnUserId = await GetCurrentTurnUserIdAsync(id, players);

            var last = await _moves.GetLastMoveAsync(id);
            int lastTurn = last?.TurnNumber ?? 0;

            string? winnerName = null;
            if (game.Status == "Completed")
            {
                winnerName = "Tie";
                if (game.WinnerGamePlayerID.HasValue)
                {
                    var winner = players.FirstOrDefault(p => p.GamePlayerID == game.WinnerGamePlayerID.Value);
                    if (winner != null) winnerName = winner.Username;
                }
            }

            return Json(new
            {
                ok = true,
                status = game.Status,
                currentTurnUserId,
                lastTurn,
                winnerName
            });
        }
    }
}
