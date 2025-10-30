using Microsoft.AspNetCore.Mvc;
using MemoryGame.Models;
using MemoryGame.Repositories;

namespace MemoryGame.Controllers
{
    // Simple admin-style CRUD over the Card table
    public class CardAdminController : Controller
    {
        private readonly ICardRepository _cards;

        public CardAdminController(ICardRepository cards)
        {
            _cards = cards;
        }

        // GET: /CardAdmin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _cards.GetAllAsync();
            return View(list);
        }

        // GET: /CardAdmin/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Card());
        }

        // POST: /CardAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Card model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Minimal sanity checks
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
            if (string.IsNullOrWhiteSpace(model.PairKey))
                ModelState.AddModelError(nameof(model.PairKey), "PairKey is required.");

            if (!ModelState.IsValid)
                return View(model);

            await _cards.CreateAsync(model);
            TempData["Info"] = "Card created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /CardAdmin/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _cards.GetByIdAsync(id);
            if (card == null)
            {
                TempData["Error"] = "Card not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(card);
        }

        // POST: /CardAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Card model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
            if (string.IsNullOrWhiteSpace(model.PairKey))
                ModelState.AddModelError(nameof(model.PairKey), "PairKey is required.");

            if (!ModelState.IsValid)
                return View(model);

            var exists = await _cards.GetByIdAsync(model.CardID);
            if (exists == null)
            {
                TempData["Error"] = "Card not found.";
                return RedirectToAction(nameof(Index));
            }

            await _cards.UpdateAsync(model);
            TempData["Info"] = "Card updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /CardAdmin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _cards.GetByIdAsync(id);
            if (exists == null)
            {
                TempData["Error"] = "Card not found.";
                return RedirectToAction(nameof(Index));
            }

            await _cards.DeleteAsync(id);
            TempData["Info"] = "Card deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
