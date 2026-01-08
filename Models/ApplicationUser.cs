using LibraryManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

    [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();


    public string FullName => $"{FirstName} {LastName}";
    }
