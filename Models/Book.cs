using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression(@"^(?:\d{10}|\d{13})$", ErrorMessage = "ISBN must consist of 10 or 13 digits")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Publication year is required")]
        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        [Display(Name = "Publication Year")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "Number of copies is required")]
        [Range(0, 1000, ErrorMessage = "Number of copies must be between 0 and 1000")]
        [Display(Name = "Number of Available Copies")]
        public int AvailableCopies { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}