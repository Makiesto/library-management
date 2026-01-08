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

            var books = await booksQuery.ToListAsync();

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

        private async Task<List<SelectListItem>> GetAuthorsSelectList()
        {
            return await _context.Authors
                .OrderBy(a => a.LastName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                })
                .ToListAsync();
        }
    }
}
