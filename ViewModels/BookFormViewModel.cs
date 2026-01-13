using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression(@"^(?:\d{10}|\d{13})$", ErrorMessage = "ISBN must be 10 or 13 digits")]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [Range(1000, 2100)]
        [Display(Name = "Publication Year")]
        public int PublicationYear { get; set; }

        [Required]
        [Range(0, 1000)]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Select at least one author")]
        public List<int> SelectedAuthorIds { get; set; } = new List<int>();

        public List<SelectListItem>? AvailableAuthors { get; set; }
    }
}
