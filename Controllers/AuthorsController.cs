using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly LibraryDbContext _context;

        public AuthorsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Authors
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var authorsQuery = _context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                authorsQuery = authorsQuery.Where(a =>
                    a.FirstName.Contains(searchString) ||
                    a.LastName.Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            var authors = await authorsQuery
                .Include(a => a.BookAuthors) 
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();

            return View(authors);
        }

        // GET: Authors/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null) return NotFound();

            return View(author);
        }

        // GET: Authors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Author added.";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors.FindAsync(id);
            if (author == null) return NotFound();

            return View(author);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Author updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null) return NotFound();

            ViewBag.HasBooks = author.BookAuthors.Any();
            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) return NotFound();

            if (author.BookAuthors.Any())
            {
                TempData["ErrorMessage"] = "You cannot remove an author who has books assigned to them.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Autor removed.";
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }
    }
}