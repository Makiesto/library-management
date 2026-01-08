using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LibraryManagement.Models
{
    
    // Represents an author in the library system
    public class Author
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation property for many-to-many relationship (Author - Book)
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }

    
    // Join entity class for many-to-many relationship between Book and Author.

    public class BookAuthor
    {
        // Foreign key to Book
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        // Foreign key to Author
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }
}