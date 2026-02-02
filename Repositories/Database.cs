using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace FinanceManager.WinForms.Repositories
{
    public static class Database
    {
        private static readonly string DbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        private static readonly string DbPath = Path.Combine(DbFolder, "finance.db");
        public static string ConnectionString => $"Data Source={DbPath}";

        public static void EnsureDatabase()
        {
            if (!Directory.Exists(DbFolder)) Directory.CreateDirectory(DbFolder);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Date TEXT NOT NULL,
                  Amount REAL NOT NULL,
                  Description TEXT,
                  Category TEXT,
                  Type TEXT
                );
                CREATE TABLE IF NOT EXISTS Users (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Username TEXT NOT NULL UNIQUE,
                  PasswordHash TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS Categories (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Name TEXT NOT NULL UNIQUE
                );
                CREATE TABLE IF NOT EXISTS Budgets (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Category TEXT,
                  Month TEXT,
                  Amount REAL
                );";
            cmd.ExecuteNonQuery();
        }
    }
}
