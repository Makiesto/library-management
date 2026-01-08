using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Loan Date")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Status")]
        public LoanStatus Status { get; set; }

        public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;
        public bool IsReturned => ReturnDate.HasValue;
    }

    public enum LoanStatus
    {
        [Display(Name = "Active")]
        Active = 1,
        
        [Display(Name = "Returned")]
        Returned = 2,
        
        [Display(Name = "Overdue")]
        Overdue = 3
    }

   
}