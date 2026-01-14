# Library Management System - ASP.NET MVC

> **[🇵🇱 Polska wersja](README.pl.md)** | **[🇬🇧 English version](README.md)**

## 📋 Project Description

Library Management System is a web application built with ASP.NET Core MVC that enables comprehensive management of library resources. The application allows for managing books, authors, users, and loans.

## 🚀 Installation

### Requirements
- .NET 8.0 SDK or newer
- SQL Server 2019 or LocalDB / SQLite
- Visual Studio 2022 or VS Code
- Git

### Installation Steps

1. **Clone the repository**
```bash
git clone https://github.com/Makiesto/library-management
cd library-management
```

2. **Install packages**
```bash
dotnet restore
```

3. **Configure database**

This project is cross-platform and supports both **SQLite** (recommended for macOS/Linux) and **SQL Server** (Windows).

#### A. Switching to SQLite (macOS/Linux Setup)
If you are on a Mac or Linux, follow these steps to use SQLite:

* **appsettings.json**: Ensure the connection string is set to a file:
```json
"DefaultConnection": "Data Source=library.db"
```
* **Program.cs**: Use the SQLite provider:
```csharp
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
```
* **LibraryDbContextFactory.cs**: Update the design-time factory:
```csharp
optionsBuilder.UseSqlite("Data Source=library.db");
```

#### B. Switching to SQL Server (Windows Setup)
If you are on Windows and prefer LocalDB:

* **appsettings.json**: Use the LocalDB connection string:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementDB;Trusted_Connection=True;"
```
* **Program.cs**: Use the SQL Server provider:
```csharp
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
```
* **LibraryDbContextFactory.cs**: Update the design-time factory:
```csharp
optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LibraryManagementDB;...");
```

4 **Running Migrations**
After configuring your preferred provider, run the following commands:

```bash
rm -rf Migrations

dotnet ef migrations add InitialCreate

dotnet ef database update
```

5. **Run application**
```bash
dotnet run
```

Application will be available at: `https://localhost:5235`

---

## 🔑 Test Users

The application automatically creates test users on first run:

### Administrator
- **Email:** `admin@library.pl`
- **Password:** `Admin123!`
- **Permissions:** Full access to all system features

### Test User
- **Email:** `user@library.pl`
- **Password:** `User123!`
- **Permissions:** Browse, search, and borrow books

---

## ⚙️ Features

### For Administrator:
- ✅ Full CRUD operations for books (create, read, update, delete)
- ✅ Manage authors
- ✅ View all loans
- ✅ User management via ASP.NET Identity
- ✅ Access to REST API

### For Regular Users:
- ✅ Browse book catalog
- ✅ Search books by title, ISBN, and author
- ✅ Borrow available books
- ✅ View personal loan history
- ✅ Return borrowed books

---

## 🗄️ Database

### Entities

1. **Book** (Main Entity)
- Id, Title, ISBN, PublicationYear, AvailableCopies, Description
- Many-to-Many relationship with Author via BookAuthor
- One-to-Many relationship with Loan

2. **Author**
- Id, FirstName, LastName, BirthDate, Biography
- Many-to-Many relationship with Book via BookAuthor

3. **ApplicationUser** (extends IdentityUser)
- FirstName, LastName, RegistrationDate
- One-to-Many relationship with Loan

4. **Loan**
- Id, BookId, UserId, LoanDate, DueDate, ReturnDate, Status
- Many-to-One relationships with Book and ApplicationUser

### Entity Relationship Diagram
```
Author <----> BookAuthor <----> Book <----> Loan <----> ApplicationUser
(Many-to-Many) (One-to-Many) (Many-to-One)
```

---

## 📋 Forms with Validation

### 1. Registration Form
- **Email** (required, email format)
- **First Name & Last Name** (required, max 100 characters)
- **Password** (min 8 characters, must contain digit, lowercase, and uppercase)
- **Confirm Password** (must match password)

### 2. Add/Edit Book Form
- **Title** (required, max 200 characters)
- **ISBN** (required, 10 or 13 digits, unique)
- **Publication Year** (required, range 1000-2100)
- **Available Copies** (required, range 0-1000)
- **Description** (optional, max 1000 characters)
- **Authors** (at least 1 required)

### 3. Loan Form
- **Book Selection** (required, only available books)
- **Loan Date** (required, cannot be in the future)
- **Due Date** (required, must be after loan date)

---

## 🔌 REST API

The application provides a complete REST API for the Book entity.

### Available Endpoints

#### 📚 GET /api/booksapi
Get all books with authors
```bash
curl https://localhost:5235/api/booksapi
```

**Response 200 OK:**
```json
[
{
"id": 1,
"title": "The Witcher: The Last Wish",
"isbn": "9788375780635",
"publicationYear": 1993,
"availableCopies": 3,
"description": "Collection of short stories...",
"authors": [
{
"id": 1,
"firstName": "Andrzej",
"lastName": "Sapkowski"
}
]
}
]
```

---

#### 📖 GET /api/booksapi/{id}
Get book details by ID
```bash
curl https://localhost:5235/api/booksapi/1
```

**Response 404 Not Found:**
```json
{
"message": "Book with ID 999 not found"
}
```

---

#### 🔍 GET /api/booksapi/search?query={text}
Search books by title or ISBN
```bash
curl "https://localhost:5235/api/booksapi/search?query=harry"
```

---

#### 📊 GET /api/booksapi/stats
Get book statistics
```bash
curl https://localhost:5235/api/booksapi/stats
```

**Response:**
```json
{
"totalBooks": 15,
"totalCopies": 47,
"availableBooks": 12,
"unavailableBooks": 3
}
```

---

#### ➕ POST /api/booksapi
Create a new book
```bash
curl -X POST https://localhost:5235/api/booksapi \
-H "Content-Type: application/json" \
-d '{
"title": "New Book",
"isbn": "1234567890",
"publicationYear": 2024,
"availableCopies": 5,
"description": "Book description",
"authorIds": [1]
}'
```

**Response 201 Created** - Returns created book with ID

**Response 409 Conflict:**
```json
{
"message": "A book with this ISBN already exists"
}
```

---

#### ✏️ PUT /api/booksapi/{id}
Update an existing book
```bash
curl -X PUT https://localhost:5235/api/booksapi/1 \
-H "Content-Type: application/json" \
-d '{
"title": "Updated Title",
"isbn": "1234567890",
"publicationYear": 2024,
"availableCopies": 10,
"authorIds": [1, 2]
}'
```

**Response 204 No Content** - Success

---

#### 🗑️ DELETE /api/booksapi/{id}
Delete a book
```bash
curl -X DELETE https://localhost:5235/api/booksapi/1
```

**Response 400 Bad Request:**
```json
{
"message": "Cannot delete a book with active loans"
}
```

---

### 🧪 API Testing

#### Option 1: Browser
Open in browser:
```
https://localhost:5235/api/booksapi
https://localhost:5235/api/booksapi/1
https://localhost:5235/api/booksapi/stats
https://localhost:5235/api/booksapi/search?query=harry
```

#### Option 2: PowerShell
```powershell
# Get all books
Invoke-RestMethod -Uri "https://localhost:5235/api/booksapi" -Method Get

# Get statistics
Invoke-RestMethod -Uri "https://localhost:5235/api/booksapi/stats" -Method Get

# Create book
$body = @{
title = "Test Book"
isbn = "1234567890"
publicationYear = 2024
availableCopies = 5
authorIds = @(1)
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5235/api/booksapi" -Method Post -Body $body -ContentType "application/json"
```

#### Option 3: Postman
1. Create new collection "Library API"
2. Add requests for each endpoint
3. Test all CRUD operations

---

## 🛠️ Technologies

### Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **ORM:** Entity Framework Core 8.0
- **Database:** SQL Server / SQLite
- **Authentication:** ASP.NET Core Identity
- **API:** RESTful Web API

### Frontend
- **Template Engine:** Razor Views
- **UI Framework:** Bootstrap 5
- **Icons:** Font Awesome
- **Validation:** jQuery Validation

### Architecture
- **Pattern:** Model-View-Controller (MVC)
- **Database Design:** Code-First with Migrations
- **Authorization:** Role-based (Admin/User)

---

## 📂 Project Structure
```
LibraryManagement/
├── Controllers/
│ ├── AccountController.cs # Authentication (Login, Register)
│ ├── BooksController.cs # Book management (CRUD)
│ ├── AuthorsController.cs # Author management (CRUD)
│ ├── LoansController.cs # Loan system
│ ├── HomeController.cs # Home page
│ └── Api/
│ └── BooksApiController.cs # REST API for Books
├── Models/
│ ├── Book.cs # Book entity
│ ├── Author.cs # Author entity + BookAuthor
│ ├── Loan.cs # Loan entity + LoanStatus enum
│ ├── ApplicationUser.cs # Extended user (Identity)
│ └── ErrorViewModel.cs # Error handling
├── ViewModels/ # ⭐ Form models with validation
│ ├── RegisterViewModel.cs # Registration form
│ ├── LoginViewModel.cs # Login form
│ ├── BookFormViewModel.cs # Book create/edit form
│ └── CreateLoanViewModel.cs # Loan creation form
├── Data/
│ ├── LibraryDbContext.cs # EF Core DbContext
│ ├── LibraryDbContextFactory.cs # Design-time DB factory (for migrations)
│ └── SeedData.cs # Initial seed data
├── Views/
│ ├── Books/ # Book views (Index, Details, Create, Edit, Delete)
│ ├── Authors/ # Author views (Index, Details, Create, Edit, Delete)
│ ├── Loans/ # Loan views (Index, Details, Create)
│ ├── Account/ # Authentication views (Login, Register)
│ ├── Home/ # Home page views
│ └── Shared/ # Shared layouts (_Layout.cshtml)
├── wwwroot/ # Static files (CSS, JS, images)
│ ├── css/
│ ├── js/
│ └── lib/
├── Migrations/ # EF Core migrations (auto-generated)
├── appsettings.json # Configuration (connection strings)
├── Program.cs # Application entry point
└── LibraryManagement.csproj # Project file
```

---

## ✅ Academic Project Requirements

This project fulfills all academic requirements:

- ✅ **MVC Pattern** - Complete separation of Model-View-Controller
- ✅ **3+ Forms with Validation** - Registration, Book Management, Loan Creation
- ✅ **Entity Framework** - Code-First approach with migrations
- ✅ **4+ Entities in Relationships** - Book, Author, User, Loan with defined relationships
- ✅ **User Authorization** - Two-level access (Admin/User)
- ✅ **REST API CRUD** - Complete API for Book entity
- ✅ **GitHub Repository** - Version control with multiple commits (2+ weeks before submission)
- ✅ **Documentation** - Comprehensive installation guide and user manual

---

## 🔒 Security Features

- **Password Hashing:** ASP.NET Core Identity secure hashing
- **Role-Based Authorization:** Granular access control
- **Anti-Forgery Tokens:** CSRF protection on all forms
- **SQL Injection Protection:** Entity Framework parameterized queries
- **XSS Prevention:** Razor automatic encoding

---

## 🚀 Deployment

### For Local Development
```bash
dotnet run
```

### For Production
```bash
dotnet publish -c Release -o ./publish
# Deploy the ./publish folder to your server
```

---

## 🐛 Troubleshooting

### Issue: "Cannot connect to database"
**Solution:** Check connection string in `appsettings.json` and ensure SQL Server/SQLite is running

### Issue: "Migrations not found"
**Solution:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue: "Cannot login"
**Solution:** Ensure database is updated and seed data was created. Check for errors in console.

### Issue: (SQLite) 'table "AspNetRoles" already exists'
**Solution:** If you receive this error during `database update`, delete the `library.db` file from the project's root folder and try again. This is because the database was initialized before any formal migrations were performed.


---

## 📝 License

Academic project - free to use for educational purposes.

---

## 👥 Authors

- **[Mateusz Stojek]** - Implementation

GitHub: [https://github.com/makiesto/LibraryManagement]

---

## 📧 Contact

For questions or issues:
- **Email:** mateusz.stojek@proton.me
- **GitHub Issues:** [Create an issue](https://github.com/makiesto/LibraryManagement/issues)

---

## 🎓 University Information

- **Course:** Bazy Danych i Aplikacje Internetowe
- **Semester:** 5
- **Year:** 2025/2026
- **Instructor:** Agnieszka Smolarek

-------------------
