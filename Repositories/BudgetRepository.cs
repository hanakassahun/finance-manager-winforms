using FinanceManager.WinForms.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Repositories
{
    public class BudgetRepository
    {
        public BudgetRepository() { Database.EnsureDatabase(); }

        public long AddOrUpdate(Budget b)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Budgets (Category, Month, Amount) VALUES ($cat,$month,$amt) ON CONFLICT(Category,Month) DO UPDATE SET Amount=$amt;";
                cmd.Parameters.AddWithValue("$cat", b.Category);
                cmd.Parameters.AddWithValue("$month", b.Month);
                cmd.Parameters.AddWithValue("$amt", b.Amount);
                cmd.ExecuteNonQuery();
            }

            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = "SELECT Id FROM Budgets WHERE Category=$cat AND Month=$month LIMIT 1;";
                cmd2.Parameters.AddWithValue("$cat", b.Category);
                cmd2.Parameters.AddWithValue("$month", b.Month);
                var res = cmd2.ExecuteScalar();
                return res == null || res is DBNull ? 0L : (long)res;
            }
        }

        public List<Budget> GetAll()
        {
            var list = new List<Budget>();
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Category, Month, Amount FROM Budgets ORDER BY Month DESC, Category;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Budget { Id = reader.GetInt64(0), Category = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), Month = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), Amount = (decimal)reader.GetDouble(3) });
            }
            return list;
        }

        public Budget? GetFor(string category, string month)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Category, Month, Amount FROM Budgets WHERE Category=$cat AND Month=$month LIMIT 1;";
            cmd.Parameters.AddWithValue("$cat", category);
            cmd.Parameters.AddWithValue("$month", month);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Budget { Id = reader.GetInt64(0), Category = reader.GetString(1), Month = reader.GetString(2), Amount = (decimal)reader.GetDouble(3) };
            }
            return null;
        }

        public void Delete(long id)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Budgets WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
