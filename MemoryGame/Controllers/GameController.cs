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

        // ================================
        // LOBBY
        // ================================
        [HttpGet]
        public async Task<IActionResult> Lobby()
        {
            var me = HttpContext.Session.GetInt32("UserID");
            var games = await _games.GetActiveGamesAsync();

            var rows = new List<LobbyRowVm>();
            foreach (var g in games)
            {
                var players = await _games.GetPlayersAsync(g.GameID);
                rows.Add(new LobbyRowVm
                {
                    GameID = g.GameID,
                    Status = g.Status,
                    CreatedAt = g.CreatedAt,
                    PlayerCount = players.Count,
                    IsParticipant = me.HasValue && players.Any(p => p.UserID == me.Value),
                    IsOwner = me.HasValue && players.Any(p => p.PlayerOrder == 1 && p.UserID == me.Value)
                });
            }

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var gameId = await _games.CreateGameAsync(userId.Value);
            return RedirectToAction("Play", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId is null) return RedirectToAction("Login", "Account");

            var ok = await _games.JoinGameAsync(id, userId.Value);
            if (!ok)
            {
                TempData["Error"] = "Cannot join: game may be full, started, or you are already in it.";
                return RedirectToAction("Lobby");
            }
            return RedirectToAction("Play", new { id });
        }

        // Owner-only delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var me = HttpContext.Session.GetInt32("UserID");
            if (me is null) return RedirectToAction("Login", "Account");

            var players = await _games.GetPlayersAsync(id);
            var owner = players.FirstOrDefault(p => p.PlayerOrder == 1);
            if (owner == null || owner.UserID != me.Value)
            {
                TempData["Error"] = "Only the game owner can delete this game.";
                return RedirectToAction("Lobby");
            }

            await _games.DeleteGameAsync(id);
            return RedirectToAction("Lobby");
        }

        // ================================
        // HELPERS
        // ================================
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

        // ================================
        // PLAY (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
            var game = await _games.GetByIdAsync(id);
            if (game == null)
            {
                TempData["Error"] = "Game not found.";
                return RedirectToAction("Lobby");
            }

            // Seed data if needed
            if (await _cards.CountAsync() == 0) await _cards.SeedDefaultPairsAsync();
            if (!await _tiles.AnyForGameAsync(id)) await _tiles.CreateForGameAsync(id, 16);

            var tiles = await _tiles.GetByGameAsync(id);

            // selection (two-click flow)
            var sel1Key = $"sel1_{id}";
            var sel1 = HttpContext.Session.GetInt32(sel1Key);

            ViewBag.GameId = id;
            ViewBag.Status = game.Status;
            ViewBag.Sel1 = sel1;

            // players, scores, history
            var players = await _games.GetPlayersAsync(id);
            var scores = await _moves.GetScoresAsync(id);
            var history = await _moves.GetHistoryAsync(id);

            ViewBag.Players = players;
            ViewBag.Scores = scores;
            ViewBag.History = history;

            // counters
            int totalTiles = tiles.Count;
            int matchedTiles = tiles.Count(t => t.IsMatched);
            int remainingPairs = (totalTiles - matchedTiles) / 2;

            ViewBag.TotalTiles = totalTiles;
            ViewBag.MatchedTiles = matchedTiles;
            ViewBag.RemainingPairs = remainingPairs;

            // if finished, compute winner and mark completed
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

            // turn + flags
            int? currentTurnUserId = null;
            if (game.Status == "InProgress" || game.Status == "Waiting")
                currentTurnUserId = await GetCurrentTurnUserIdAsync(id, players);

            var me = HttpContext.Session.GetInt32("UserID");
            ViewBag.MeUserId = me;
            ViewBag.CurrentTurnUserId = currentTurnUserId;
            ViewBag.IsMyTurn = (me.HasValue && currentTurnUserId.HasValue && me.Value == currentTurnUserId.Value);
            ViewBag.GameCompleted = (game.Status == "Completed");

            // Solo-mode failsafe
            if ((players?.Count ?? 0) == 1 && me.HasValue && me.Value == players[0].UserID && !(bool)ViewBag.GameCompleted)
            {
                ViewBag.IsMyTurn = true;
            }

            // winner (for display)
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

            // expose last turn number for polling
            var last = await _moves.GetLastMoveAsync(id);
            ViewBag.LastTurn = last?.TurnNumber ?? 0;

            // keep last pair through one render
            var lastPair = TempData["LastPair"];
            if (lastPair != null) TempData["LastPair"] = lastPair;

            return View(tiles);
        }

        // ================================
        // FLIP (POST)
        // ================================
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

            // Enforce turn
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

        // ================================
        // STATE (JSON for polling)
        // ================================
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

        // Rematch endpoint removed
    }
}
