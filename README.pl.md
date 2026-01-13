# System Zarządzania Biblioteką - ASP.NET MVC

> **[🇵🇱 Polska wersja](README.pl.md)** | **[🇬🇧 English version](README.md)**

## 📋 Opis projektu

System Zarządzania Biblioteką to aplikacja webowa stworzona w ASP.NET Core MVC, umożliwiająca kompleksowe zarządzanie zasobami bibliotecznymi. Aplikacja pozwala na zarządzanie książkami, autorami, użytkownikami oraz wypożyczeniami.

## 🚀 Instalacja

### Wymagania
- .NET 8.0 SDK lub nowszy
- SQL Server 2019 lub LocalDB / SQLite
- Visual Studio 2022 lub VS Code
- Git

### Kroki instalacji

1. **Sklonuj repozytorium**
```bash
git clone https://github.com/makiesto/LibraryManagement.git
cd LibraryManagement
```

2. **Zainstaluj pakiety**
```bash
dotnet restore
```

3. **Skonfiguruj bazę danych**

Edytuj `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Dla SQLite (alternatywa):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=library.db"
  }
}
```

4. **Utwórz bazę danych**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. **Uruchom aplikację**
```bash
dotnet run
```

Aplikacja będzie dostępna pod adresem: `https://localhost:5001`

---

## 🔑 Użytkownicy testowi

Aplikacja automatycznie tworzy użytkowników testowych przy pierwszym uruchomieniu:

### Administrator
- **Email:** `admin@library.pl`
- **Hasło:** `Admin123!`
- **Uprawnienia:** Pełny dostęp do wszystkich funkcji systemu

### Użytkownik testowy
- **Email:** `user@library.pl`
- **Hasło:** `User123!`
- **Uprawnienia:** Przeglądanie, wyszukiwanie i wypożyczanie książek

---

## ⚙️ Funkcjonalności

### Dla Administratora:
- ✅ Pełne operacje CRUD dla książek (tworzenie, odczyt, aktualizacja, usuwanie)
- ✅ Zarządzanie autorami
- ✅ Przeglądanie wszystkich wypożyczeń
- ✅ Zarządzanie użytkownikami przez ASP.NET Identity
- ✅ Dostęp do REST API

### Dla Zwykłych Użytkowników:
- ✅ Przeglądanie katalogu książek
- ✅ Wyszukiwanie książek po tytule, ISBN i autorze
- ✅ Wypożyczanie dostępnych książek
- ✅ Przeglądanie historii własnych wypożyczeń
- ✅ Zwracanie wypożyczonych książek

---

## 🗄️ Baza danych

### Encje

1. **Book** (Główna encja)
   - Id, Title, ISBN, PublicationYear, AvailableCopies, Description
   - Relacja Many-to-Many z Author przez BookAuthor
   - Relacja One-to-Many z Loan

2. **Author**
   - Id, FirstName, LastName, BirthDate, Biography
   - Relacja Many-to-Many z Book przez BookAuthor

3. **ApplicationUser** (rozszerza IdentityUser)
   - FirstName, LastName, RegistrationDate
   - Relacja One-to-Many z Loan

4. **Loan**
   - Id, BookId, UserId, LoanDate, DueDate, ReturnDate, Status
   - Relacje Many-to-One z Book i ApplicationUser

### Diagram relacji encji
```
Author <----> BookAuthor <----> Book <----> Loan <----> ApplicationUser
       (Many-to-Many)        (One-to-Many)   (Many-to-One)
```

---

## 📋 Formularze z walidacją

### 1. Formularz rejestracji
- **Email** (wymagany, format email)
- **Imię i Nazwisko** (wymagane, max 100 znaków)
- **Hasło** (min 8 znaków, musi zawierać cyfrę, małą i wielką literę)
- **Potwierdzenie hasła** (musi być zgodne z hasłem)

### 2. Formularz dodawania/edycji książki
- **Tytuł** (wymagany, max 200 znaków)
- **ISBN** (wymagany, 10 lub 13 cyfr, unikalny)
- **Rok wydania** (wymagany, zakres 1000-2100)
- **Liczba dostępnych kopii** (wymagana, zakres 0-1000)
- **Opis** (opcjonalny, max 1000 znaków)
- **Autorzy** (wymagany co najmniej 1)

### 3. Formularz wypożyczenia
- **Wybór książki** (wymagany, tylko dostępne książki)
- **Data wypożyczenia** (wymagana, nie może być w przyszłości)
- **Termin zwrotu** (wymagany, musi być po dacie wypożyczenia)

---

## 🔌 REST API

Aplikacja udostępnia kompletne REST API dla encji Book.

### Dostępne endpointy

#### 📚 GET /api/booksapi
Pobierz wszystkie książki z autorami
```bash
curl https://localhost:5001/api/booksapi
```

**Odpowiedź 200 OK:**
```json
[
  {
    "id": 1,
    "title": "Wiedźmin: Ostatnie życzenie",
    "isbn": "9788375780635",
    "publicationYear": 1993,
    "availableCopies": 3,
    "description": "Zbiór opowiadań...",
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
Pobierz szczegóły książki po ID
```bash
curl https://localhost:5001/api/booksapi/1
```

**Odpowiedź 404 Not Found:**
```json
{
  "message": "Książka o ID 999 nie została znaleziona"
}
```

---

#### 🔍 GET /api/booksapi/search?query={text}
Wyszukaj książki po tytule lub ISBN
```bash
curl "https://localhost:5001/api/booksapi/search?query=harry"
```

---

#### 📊 GET /api/booksapi/stats
Pobierz statystyki książek
```bash
curl https://localhost:5001/api/booksapi/stats
```

**Odpowiedź:**
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
Utwórz nową książkę
```bash
curl -X POST https://localhost:5001/api/booksapi \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Nowa książka",
    "isbn": "1234567890",
    "publicationYear": 2024,
    "availableCopies": 5,
    "description": "Opis książki",
    "authorIds": [1]
  }'
```

**Odpowiedź 201 Created** - Zwraca utworzoną książkę z ID

**Odpowiedź 409 Conflict:**
```json
{
  "message": "Książka o tym ISBN już istnieje"
}
```

---

#### ✏️ PUT /api/booksapi/{id}
Zaktualizuj istniejącą książkę
```bash
curl -X PUT https://localhost:5001/api/booksapi/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Zaktualizowany tytuł",
    "isbn": "1234567890",
    "publicationYear": 2024,
    "availableCopies": 10,
    "authorIds": [1, 2]
  }'
```

**Odpowiedź 204 No Content** - Sukces

---

#### 🗑️ DELETE /api/booksapi/{id}
Usuń książkę
```bash
curl -X DELETE https://localhost:5001/api/booksapi/1
```

**Odpowiedź 400 Bad Request:**
```json
{
  "message": "Nie można usunąć książki z aktywnymi wypożyczeniami"
}
```

---

### 🧪 Testowanie API

#### Opcja 1: Przeglądarka
Otwórz w przeglądarce:
```
https://localhost:5001/api/booksapi
https://localhost:5001/api/booksapi/1
https://localhost:5001/api/booksapi/stats
https://localhost:5001/api/booksapi/search?query=harry
```

#### Opcja 2: PowerShell
```powershell
# Pobierz wszystkie książki
Invoke-RestMethod -Uri "https://localhost:5001/api/booksapi" -Method Get

# Pobierz statystyki
Invoke-RestMethod -Uri "https://localhost:5001/api/booksapi/stats" -Method Get

# Utwórz książkę
$body = @{
    title = "Testowa książka"
    isbn = "1234567890"
    publicationYear = 2024
    availableCopies = 5
    authorIds = @(1)
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/booksapi" -Method Post -Body $body -ContentType "application/json"
```

#### Opcja 3: Postman
1. Utwórz nową kolekcję "Library API"
2. Dodaj requesty dla każdego endpointa
3. Testuj wszystkie operacje CRUD

---

## 🛠️ Technologie

### Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **ORM:** Entity Framework Core 8.0
- **Baza danych:** SQL Server / SQLite
- **Autoryzacja:** ASP.NET Core Identity
- **API:** RESTful Web API

### Frontend
- **Silnik szablonów:** Razor Views
- **Framework UI:** Bootstrap 5
- **Ikony:** Font Awesome
- **Walidacja:** jQuery Validation

### Architektura
- **Wzorzec:** Model-View-Controller (MVC)
- **Projektowanie bazy:** Code-First z migracjami
- **Autoryzacja:** Oparta na rolach (Admin/User)

---

## 📂 Struktura projektu
```
LibraryManagement/
├── Controllers/
│   ├── AccountController.cs       # Autoryzacja (logowanie, rejestracja)
│   ├── BooksController.cs         # Zarządzanie książkami (CRUD)
│   ├── AuthorsController.cs       # Zarządzanie autorami (CRUD)
│   ├── LoansController.cs         # System wypożyczeń
│   ├── HomeController.cs          # Strona główna
│   └── Api/
│       └── BooksApiController.cs  # REST API dla książek
├── Models/
│   ├── Book.cs                    # Encja książki
│   ├── Author.cs                  # Encja autora + BookAuthor
│   ├── Loan.cs                    # Encja wypożyczenia + LoanStatus
│   ├── ApplicationUser.cs         # Rozszerzony użytkownik (Identity)
│   └── ErrorViewModel.cs          # Obsługa błędów
├── ViewModels/                    # ⭐ Modele formularzy z walidacją
│   ├── RegisterViewModel.cs       # Formularz rejestracji
│   ├── LoginViewModel.cs          # Formularz logowania
│   ├── BookFormViewModel.cs       # Formularz książki (dodaj/edytuj)
│   └── CreateLoanViewModel.cs     # Formularz wypożyczenia
├── Data/
│   ├── LibraryDbContext.cs        # Kontekst EF Core
│   ├── LibraryDbContextFactory.cs # Factory dla migracji (design-time)
│   └── SeedData.cs                # Dane początkowe
├── Views/
│   ├── Books/                     # Widoki książek (Index, Details, Create, Edit, Delete)
│   ├── Authors/                   # Widoki autorów (Index, Details, Create, Edit, Delete)
│   ├── Loans/                     # Widoki wypożyczeń (Index, Details, Create)
│   ├── Account/                   # Widoki autoryzacji (Login, Register)
│   ├── Home/                      # Widoki strony głównej
│   └── Shared/                    # Współdzielone layouty (_Layout.cshtml)
├── wwwroot/                       # Pliki statyczne (CSS, JS, obrazy)
│   ├── css/
│   ├── js/
│   └── lib/
├── Migrations/                    # Migracje EF Core (automatyczne)
├── appsettings.json               # Konfiguracja (connection strings)
├── Program.cs                     # Punkt wejścia aplikacji
└── LibraryManagement.csproj       # Plik projektu
```

---

## ✅ Wymagania projektu akademickiego

Projekt spełnia wszystkie wymagania akademickie:

- ✅ **Wzorzec MVC** - Kompletna separacja Model-View-Controller
- ✅ **3+ formularze z walidacją** - Rejestracja, Zarządzanie książkami, Tworzenie wypożyczenia
- ✅ **Entity Framework** - Podejście Code-First z migracjami
- ✅ **4+ encje w relacjach** - Book, Author, User, Loan ze zdefiniowanymi relacjami
- ✅ **Autoryzacja użytkowników** - Dostęp dwupoziomowy (Admin/User)
- ✅ **REST API CRUD** - Kompletne API dla encji Book
- ✅ **Repozytorium GitHub** - Kontrola wersji z wieloma commitami (2+ tygodnie przed oddaniem)
- ✅ **Dokumentacja** - Kompleksowy przewodnik instalacji i instrukcja użytkownika

---

## 🔒 Funkcje bezpieczeństwa

- **Hashowanie haseł:** Bezpieczne hashowanie ASP.NET Core Identity
- **Autoryzacja oparta na rolach:** Szczegółowa kontrola dostępu
- **Tokeny Anti-Forgery:** Ochrona CSRF we wszystkich formularzach
- **Ochrona przed SQL Injection:** Zapytania parametryzowane Entity Framework
- **Zapobieganie XSS:** Automatyczne kodowanie Razor

---

## 🚀 Wdrożenie

### Dla lokalnego developmentu
```bash
dotnet run
```

### Dla produkcji
```bash
dotnet publish -c Release -o ./publish
# Wdróż folder ./publish na swój serwer
```

---

## 🐛 Rozwiązywanie problemów

### Problem: "Nie można połączyć się z bazą danych"
**Rozwiązanie:** Sprawdź connection string w `appsettings.json` i upewnij się, że SQL Server/SQLite działa

### Problem: "Migracje nie zostały znalezione"
**Rozwiązanie:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Problem: "Nie można się zalogować"
**Rozwiązanie:** Upewnij się, że baza danych została zaktualizowana i dane początkowe zostały utworzone. Sprawdź błędy w konsoli.

---

## 📝 Licencja

Projekt akademicki - wolny do użytku w celach edukacyjnych.

---

## 👥 Autorzy

- **Mateusz Stojek** - Implementacja

GitHub: [https://github.com/makiesto/LibraryManagement](https://github.com/makiesto/LibraryManagement)

---

## 📧 Kontakt

W razie pytań lub problemów:
- **Email:** mateusz.stojek@proton.me
- **GitHub Issues:** [Zgłoś problem](https://github.com/makiesto/LibraryManagement/issues)

---

## 🎓 Informacje o uczelni

- **Przedmiot:** Bazy Danych i Aplikacje Internetowe
- **Semestr:** 5
- **Rok:** 2025/2026
- **Prowadzący:** Agnieszka Smolarek
