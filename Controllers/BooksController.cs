using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString)
        {
            var booksQuery = _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.ISBN.Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            var books = await booksQuery
               .OrderBy(b => b.Title)
               .ToListAsync();

            return View(books);
        }

        // GET: Books/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new BookFormViewModel
            {
                AvailableAuthors = await GetAuthorsSelectList()
            };
            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Books.AnyAsync(b => b.ISBN == model.ISBN))
                {
                    ModelState.AddModelError("ISBN", "A book with this ISBN already exists.");
                    model.AvailableAuthors = await GetAuthorsSelectList();
                    return View(model);
                }

                var book = new Book
                {
                    Title = model.Title,
                    ISBN = model.ISBN,
                    PublicationYear = model.PublicationYear,
                    AvailableCopies = model.AvailableCopies,
                    Description = model.Description
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                foreach (var authorId in model.SelectedAuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = authorId
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Book has been added.";
                return RedirectToAction(nameof(Index));
            }

            model.AvailableAuthors = await GetAuthorsSelectList();
            return View(model);
        }


        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            var viewModel = new BookFormViewModel
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                AvailableCopies = book.AvailableCopies,
                Description = book.Description,
                SelectedAuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList(),
                AvailableAuthors = await GetAuthorsSelectList()
            };

            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, BookFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _context.Books
                        .Include(b => b.BookAuthors)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (book == null) return NotFound();

                    if (await _context.Books.AnyAsync(b => b.ISBN == model.ISBN && b.Id != id))
                    {
                        ModelState.AddModelError("ISBN", "A book with this ISBN already exists.");
                        model.AvailableAuthors = await GetAuthorsSelectList();
                        return View(model);
                    }

                    book.Title = model.Title;
                    book.ISBN = model.ISBN;
                    book.PublicationYear = model.PublicationYear;
                    book.AvailableCopies = model.AvailableCopies;
                    book.Description = model.Description;

                    _context.BookAuthors.RemoveRange(book.BookAuthors);

                    foreach (var authorId in model.SelectedAuthorIds)
                    {
                        _context.BookAuthors.Add(new BookAuthor
                        {
                            BookId = book.Id,
                            AuthorId = authorId
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Book has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BookExists(model.Id))
                        return NotFound();
                    throw;
                }
            }

            model.AvailableAuthors = await GetAuthorsSelectList();
            return View(model);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.Loans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            ViewBag.HasActiveLoans = book.Loans.Any(l => l.ReturnDate == null);
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books
                .Include(b => b.Loans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            // Check for active loans
            if (book.Loans.Any(l => l.ReturnDate == null))
            {
                TempData["ErrorMessage"] = "Cannot delete a book with active loans.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<bool> BookExists(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }

        private async Task<List<SelectListItem>> GetAuthorsSelectList()
        {
            return await _context.Authors
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                })
                .ToListAsync();
        }
    }
}
