using Microsoft.AspNetCore.Identity;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagement.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@library.pl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Libraries",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var userEmail = "user@library.pl";
            var testUser = await userManager.FindByEmailAsync(userEmail);

            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(testUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "User");
                }
            }

            var context = serviceProvider.GetRequiredService<LibraryDbContext>();

            if (!context.Authors.Any())
            {
                var authors = new[]
                {
                    new Author { FirstName = "Andrzej", LastName = "Sapkowski", BirthDate = new DateTime(1948, 6, 21) },
                    new Author { FirstName = "J.K.", LastName = "Rowling", BirthDate = new DateTime(1965, 7, 31) },
                    new Author { FirstName = "Stephen", LastName = "King", BirthDate = new DateTime(1947, 9, 21) }
                };

                context.Authors.AddRange(authors);
                await context.SaveChangesAsync();
            }

            if (!context.Books.Any())
            {
                var sapkowski = await context.Authors.FirstAsync(a => a.LastName == "Sapkowski");
                var rowling = await context.Authors.FirstAsync(a => a.LastName == "Rowling");

                var books = new[]
                {
                    new Book
                    {
                        Title = "Wiedzmin: Ostatnie Zyczenie",
                        ISBN = "9788375780635",
                        PublicationYear = 1993,
                        AvailableCopies = 3,
                        Description = "Zbior opowiadan o wiedzminie Geralcie"
                    },
                    new Book
                    {
                        Title = "Harry Potter i Kamien Filozoficzny",
                        ISBN = "9788380082445",
                        PublicationYear = 1997,
                        AvailableCopies = 5,
                        Description = "Pierwsza czesc przygod mlodego czarodzieja"
                    }
                };

                context.Books.AddRange(books);
                await context.SaveChangesAsync();

                context.BookAuthors.AddRange(
                    new BookAuthor { BookId = books[0].Id, AuthorId = sapkowski.Id },
                    new BookAuthor { BookId = books[1].Id, AuthorId = rowling.Id }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}