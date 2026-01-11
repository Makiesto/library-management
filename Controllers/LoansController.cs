using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoansController(LibraryDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Loan> loansQuery = _context.Loans
                .Include(l => l.Book)
                .ThenInclude(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(l => l.User);

            if (!isAdmin)
            {
                loansQuery = loansQuery.Where(l => l.UserId == user.Id);
            }

            var loans = await loansQuery
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            foreach (var loan in loans.Where(l => l.ReturnDate == null && l.Status != LoanStatus.Overdue))
            {
                if (loan.IsOverdue)
                {
                    loan.Status = LoanStatus.Overdue;
                }
            }
            await _context.SaveChangesAsync();

            ViewBag.IsAdmin = isAdmin;
            return View(loans);
        }

        // GET: Loans/Create
        public async Task<IActionResult> Create(int? bookId)
        {
            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .OrderBy(b => b.Title)
                .ToListAsync();

            var viewModel = new CreateLoanViewModel
            {
                BookId = bookId ?? 0,
                AvailableBooks = availableBooks.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Title} - {string.Join(", ", b.BookAuthors.Select(ba => ba.Author.FullName))} (Available: {b.AvailableCopies})",
                    Selected = b.Id == bookId
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLoanViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(viewModel.BookId);

                if (book == null)
                {
                    ModelState.AddModelError("", "Selected book does not exist.");
                }
                else if (book.AvailableCopies <= 0)
                {
                    ModelState.AddModelError("", "All copies of this book are currently on loan.");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);

                    var existingLoan = await _context.Loans
                        .Where(l => l.UserId == user.Id && l.BookId == viewModel.BookId && l.ReturnDate == null)
                        .FirstOrDefaultAsync();

                    if (existingLoan != null)
                    {
                        ModelState.AddModelError("", "You already have this book on loan.");
                    }
                    else
                    {
                        var loan = new Loan
                        {
                            BookId = viewModel.BookId,
                            UserId = user.Id,
                            LoanDate = viewModel.LoanDate,
                            DueDate = viewModel.DueDate,
                            Status = LoanStatus.Active
                        };

                        book.AvailableCopies--;

                        _context.Loans.Add(loan);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Book has been borrowed successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .OrderBy(b => b.Title)
                .ToListAsync();

            viewModel.AvailableBooks = availableBooks.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.Title} - {string.Join(", ", b.BookAuthors.Select(ba => ba.Author.FullName))} (Available: {b.AvailableCopies})"
            }).ToList();

            return View(viewModel);
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && loan.UserId != user.Id)
            {
                return Forbid();
            }

            if (loan.ReturnDate.HasValue)
            {
                TempData["ErrorMessage"] = "This book has already been returned.";
                return RedirectToAction(nameof(Index));
            }

            loan.ReturnDate = DateTime.Now;
            loan.Status = LoanStatus.Returned;
            loan.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book has been returned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .ThenInclude(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && loan.UserId != user.Id)
            {
                return Forbid();
            }

            return View(loan);
        }

        // POST: Loans/Delete/5 - Admin only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            if (!loan.ReturnDate.HasValue)
            {
                loan.Book.AvailableCopies++;
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Loan has been deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}