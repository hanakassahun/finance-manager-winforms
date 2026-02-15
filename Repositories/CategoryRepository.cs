using FinanceManager.WinForms.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Repositories
{
    public class CategoryRepository
    {
        public CategoryRepository() { Database.EnsureDatabase(); }

        public long Add(Category c)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Categories (Name) VALUES ($name); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", c.Name);
            var id = (long)cmd.ExecuteScalar();
            return id;
        }

        public List<Category> GetAll()
        {
            var list = new List<Category>();
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Categories ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category { Id = reader.GetInt64(0), Name = reader.GetString(1) });
            }
            return list;
        }

        public void Delete(long id)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Categories WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public void Update(long id, string name)
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE OR IGNORE Categories SET Name=$name WHERE Id=$id";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
