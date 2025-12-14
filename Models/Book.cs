using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł nie może być dłuższy niż 200 znaków")]
        [Display(Name = "Tytuł")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ISBN jest wymagany")]
        [RegularExpression(@"^(?:\d{10}|\d{13})$", ErrorMessage = "ISBN musi składać się z 10 lub 13 cyfr")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Rok wydania jest wymagany")]
        [Range(1000, 2100, ErrorMessage = "Rok wydania musi być pomiędzy 1000 a 2100")]
        [Display(Name = "Rok wydania")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "Liczba kopii jest wymagana")]
        [Range(0, 1000, ErrorMessage = "Liczba kopii musi być pomiędzy 0 a 1000")]
        [Display(Name = "Liczba dostępnych kopii")]
        public int AvailableCopies { get; set; }

        [StringLength(1000, ErrorMessage = "Opis nie może być dłuższy niż 1000 znaków")]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}