using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            ViewBag.Authors = await _context.Authors
                .OrderBy(a => a.LastName)
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToListAsync();
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book, List<int> authorIds)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                foreach (var authorId in authorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = authorId
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Książka została dodana.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Authors = await _context.Authors
                .OrderBy(a => a.LastName)
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToListAsync();
            return View(book);
        }

    }
}
