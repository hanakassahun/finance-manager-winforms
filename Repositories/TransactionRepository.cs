using FinanceManager.WinForms.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Repositories
{
    public class TransactionRepository
    {
        public TransactionRepository() { Database.EnsureDatabase(); }

        public long Add(Transaction t)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Transactions (Date, Amount, Description, Category, Type) VALUES ($date,$amt,$desc,$cat,$type); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$date", t.Date.ToString("o"));
            cmd.Parameters.AddWithValue("$amt", t.Amount);
            cmd.Parameters.AddWithValue("$desc", t.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("$cat", t.Category ?? string.Empty);
            cmd.Parameters.AddWithValue("$type", t.Type ?? "Expense");
            var id = (long)cmd.ExecuteScalar();
            return id;
        }

        public List<Transaction> GetAll()
        {
            var list = new List<Transaction>();
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Date, Amount, Description, Category, Type FROM Transactions ORDER BY Date DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var t = new Transaction
                {
                    Id = reader.GetInt64(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    Amount = (decimal)reader.GetDouble(2),
                    Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Category = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Type = reader.IsDBNull(5) ? "Expense" : reader.GetString(5)
                };
                list.Add(t);
            }
            return list;
        }

        public void Update(Transaction t)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Transactions SET Date=$date, Amount=$amt, Description=$desc, Category=$cat, Type=$type WHERE Id=$id";
            cmd.Parameters.AddWithValue("$date", t.Date.ToString("o"));
            cmd.Parameters.AddWithValue("$amt", t.Amount);
            cmd.Parameters.AddWithValue("$desc", t.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("$cat", t.Category ?? string.Empty);
            cmd.Parameters.AddWithValue("$type", t.Type ?? "Expense");
            cmd.Parameters.AddWithValue("$id", t.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Transactions WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public decimal GetTotalForCategoryMonth(string category, string month)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            // month format: YYYY-MM
            cmd.CommandText = "SELECT IFNULL(SUM(Amount), 0) FROM Transactions WHERE Category=$cat AND strftime('%Y-%m', Date)= $month";
            cmd.Parameters.AddWithValue("$cat", category ?? string.Empty);
            cmd.Parameters.AddWithValue("$month", month);
            var res = cmd.ExecuteScalar();
            if (res == null || res is DBNull) return 0m;
            return (decimal)Convert.ToDecimal(res);
        }
    }
}
