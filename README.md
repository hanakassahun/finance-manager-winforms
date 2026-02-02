# Finance Manager (WinForms)

This repository is a portfolio-grade WinForms finance manager scaffold built with C# and SQLite.

Features included in this scaffold:
- Clean architecture folders: `Models`, `Repositories`, `Services`, `UI`, `Utils`.
- SQLite persistence via `Microsoft.Data.Sqlite`.
- BCrypt password hashing via `BCrypt.Net-Next` (Auth service stub).
- Transaction CRUD, Add / View (DataGrid) / Edit / Delete flows.
- Simple theme manager (Light / Dark) and modern UI intent.

Quick start

1. Open this folder in Visual Studio (or run `dotnet restore` and `dotnet build`).

```bash
dotnet restore
dotnet build
dotnet run --project FinanceManager.WinForms.csproj
```

The app creates a `data/finance.db` SQLite file next to the executable.

Next steps (planned): Categories, Budgets & Alerts, Search/Filters, Charts, Figma UI mockups, and tests. Follow the TODO list in the project root.
# finance-manager-winforms
# Finance Manager - WinForms Desktop App

A simple and intuitive personal finance management system built using C# and WinForms. This desktop application helps users track expenses, manage income, categorize transactions, and generate budget reports.

## 🔧 Features

- 🧾 Add, edit, and delete income/expense entries
- 📊 Visual reports and pie chart breakdowns
- 🗂 Categorization of transactions
- 💾 Data saved locally using a lightweight database (e.g., SQL Server / SQLite)
- 👥 Multi-user login (optional, if implemented)

## 💻 Tech Stack

- C# (.NET Framework)
- WinForms (Windows Forms)
- [Database used: SQLite / SQL Server / Other]

 

