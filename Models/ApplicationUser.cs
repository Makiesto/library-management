using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";
    }
