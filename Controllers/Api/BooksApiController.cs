using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksApiController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BooksApiController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/BooksApi
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublicationYear = b.PublicationYear,
                    AvailableCopies = b.AvailableCopies,
                    Description = b.Description,
                    Authors = b.BookAuthors.Select(ba => new AuthorDto
                    {
                        Id = ba.Author.Id,
                        FirstName = ba.Author.FirstName,
                        LastName = ba.Author.LastName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(books);
        }

        // GET: api/BooksApi/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                AvailableCopies = book.AvailableCopies,
                Description = book.Description,
                Authors = book.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList()
            };

            return Ok(bookDto);
        }

        // GET: api/BooksApi/search?query=harry
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query cannot be empty" });
            }

            var books = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(b => b.Title.Contains(query) || b.ISBN.Contains(query))
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublicationYear = b.PublicationYear,
                    AvailableCopies = b.AvailableCopies,
                    Description = b.Description,
                    Authors = b.BookAuthors.Select(ba => new AuthorDto
                    {
                        Id = ba.Author.Id,
                        FirstName = ba.Author.FirstName,
                        LastName = ba.Author.LastName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(books);
        }

        // POST: api/BooksApi
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDto>> PostBook(CreateBookDto createBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Books.AnyAsync(b => b.ISBN == createBookDto.ISBN))
            {
                return Conflict(new { message = "A book with this ISBN already exists" });
            }

            if (createBookDto.AuthorIds != null && createBookDto.AuthorIds.Any())
            {
                var existingAuthorIds = await _context.Authors
                    .Where(a => createBookDto.AuthorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidAuthorIds = createBookDto.AuthorIds.Except(existingAuthorIds).ToList();
                if (invalidAuthorIds.Any())
                {
                    return BadRequest(new { message = $"Authors with IDs {string.Join(", ", invalidAuthorIds)} do not exist" });
                }
            }

            var book = new Book
            {
                Title = createBookDto.Title,
                ISBN = createBookDto.ISBN,
                PublicationYear = createBookDto.PublicationYear,
                AvailableCopies = createBookDto.AvailableCopies,
                Description = createBookDto.Description
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (createBookDto.AuthorIds != null && createBookDto.AuthorIds.Any())
            {
                foreach (var authorId in createBookDto.AuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = authorId
                    });
                }
                await _context.SaveChangesAsync();
            }

            await _context.Entry(book)
                .Collection(b => b.BookAuthors)
                .Query()
                .Include(ba => ba.Author)
                .LoadAsync();

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                AvailableCopies = book.AvailableCopies,
                Description = book.Description,
                Authors = book.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList()
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        // PUT: api/BooksApi/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutBook(int id, UpdateBookDto updateBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            if (await _context.Books.AnyAsync(b => b.ISBN == updateBookDto.ISBN && b.Id != id))
            {
                return Conflict(new { message = "A book with this ISBN already exists" });
            }

            if (updateBookDto.AuthorIds != null && updateBookDto.AuthorIds.Any())
            {
                var existingAuthorIds = await _context.Authors
                    .Where(a => updateBookDto.AuthorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidAuthorIds = updateBookDto.AuthorIds.Except(existingAuthorIds).ToList();
                if (invalidAuthorIds.Any())
                {
                    return BadRequest(new { message = $"Authors with IDs {string.Join(", ", invalidAuthorIds)} do not exist" });
                }
            }

            book.Title = updateBookDto.Title;
            book.ISBN = updateBookDto.ISBN;
            book.PublicationYear = updateBookDto.PublicationYear;
            book.AvailableCopies = updateBookDto.AvailableCopies;
            book.Description = updateBookDto.Description;

            if (updateBookDto.AuthorIds != null)
            {
                _context.BookAuthors.RemoveRange(book.BookAuthors);

                foreach (var authorId in updateBookDto.AuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = authorId
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/BooksApi/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Loans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            if (book.Loans.Any(l => l.ReturnDate == null))
            {
                return BadRequest(new { message = "Cannot delete a book with active loans" });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/BooksApi/stats
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetStats()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalCopies = await _context.Books.SumAsync(b => b.AvailableCopies);
            var availableBooks = await _context.Books.CountAsync(b => b.AvailableCopies > 0);
            var unavailableBooks = totalBooks - availableBooks;

            return Ok(new
            {
                totalBooks,
                totalCopies,
                availableBooks,
                unavailableBooks
            });
        }

        private async Task<bool> BookExists(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }
    }

    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public int PublicationYear { get; set; }
        public int AvailableCopies { get; set; }
        public string? Description { get; set; }
        public List<AuthorDto> Authors { get; set; } = new List<AuthorDto>();
    }

    public class AuthorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CreateBookDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?:\d{10}|\d{13})$")]
        public string ISBN { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1000, 2100)]
        public int PublicationYear { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(0, 1000)]
        public int AvailableCopies { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string? Description { get; set; }

        public List<int>? AuthorIds { get; set; }
    }

    public class UpdateBookDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?:\d{10}|\d{13})$")]
        public string ISBN { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1000, 2100)]
        public int PublicationYear { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(0, 1000)]
        public int AvailableCopies { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string? Description { get; set; }

        public List<int>? AuthorIds { get; set; }
    }
}