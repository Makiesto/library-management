using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.ViewModels
{
    public class CreateLoanViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please select a book")]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Loan date is required")]
        [Display(Name = "Loan Date")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        public List<SelectListItem>? AvailableBooks { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DueDate <= LoanDate)
            {
                yield return new ValidationResult(
                    "Due date must be after loan date",
                    new[] { nameof(DueDate) });
            }

            if (LoanDate > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Loan date cannot be in the future",
                    new[] { nameof(LoanDate) });
            }

            if (DueDate < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Due date cannot be in the past",
                    new[] { nameof(DueDate) });
            }
        }
    }
}