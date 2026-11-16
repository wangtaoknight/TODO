using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using TODO.Models;

namespace TODO.Data
{
    public sealed class SqliteTaskRepository
    {
        private const string DateFormat = "o";
        private readonly string _databasePath;
        private readonly string _connectionString;

        public SqliteTaskRepository(string databasePath = null)
        {
            _databasePath = string.IsNullOrWhiteSpace(databasePath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TODO", "tasks.db")
                : databasePath;

            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};Version=3;foreign keys=true;", _databasePath);
            Initialize();
        }

        public string DatabasePath { get { return _databasePath; } }

        public void Initialize()
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
CREATE TABLE IF NOT EXISTS Tasks (
    Id TEXT PRIMARY KEY NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    Priority INTEGER NOT NULL DEFAULT 2,
    DueDate TEXT NULL,
    Category TEXT NOT NULL DEFAULT '默认',
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    CompletedAt TEXT NULL
);";
                command.ExecuteNonQuery();
            }
        }

        public IList<TodoTask> GetAll()
        {
            var tasks = new List<TodoTask>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Title, Description, IsCompleted, Priority, DueDate, Category, CreatedAt, UpdatedAt, CompletedAt FROM Tasks ORDER BY IsCompleted, DueDate IS NULL, DueDate, CreatedAt DESC";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(ReadTask(reader));
                    }
                }
            }

            return tasks;
        }

        public void Insert(TodoTask task)
        {
            if (task == null) throw new ArgumentNullException("task");
            ExecuteTaskCommand(task, @"
INSERT INTO Tasks (Id, Title, Description, IsCompleted, Priority, DueDate, Category, CreatedAt, UpdatedAt, CompletedAt)
VALUES (@Id, @Title, @Description, @IsCompleted, @Priority, @DueDate, @Category, @CreatedAt, @UpdatedAt, @CompletedAt);");
        }

        public void Update(TodoTask task)
        {
            if (task == null) throw new ArgumentNullException("task");
            ExecuteTaskCommand(task, @"
UPDATE Tasks SET Title=@Title, Description=@Description, IsCompleted=@IsCompleted,
Priority=@Priority, DueDate=@DueDate, Category=@Category, CreatedAt=@CreatedAt,
UpdatedAt=@UpdatedAt, CompletedAt=@CompletedAt WHERE Id=@Id;");
        }

        public void Delete(Guid id)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Tasks WHERE Id=@Id;";
                command.Parameters.AddWithValue("@Id", id.ToString("D"));
                command.ExecuteNonQuery();
            }
        }

        private void ExecuteTaskCommand(TodoTask task, string sql)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", task.Id.ToString("D"));
                command.Parameters.AddWithValue("@Title", task.Title.Trim());
                command.Parameters.AddWithValue("@Description", task.Description ?? string.Empty);
                command.Parameters.AddWithValue("@IsCompleted", task.IsCompleted ? 1 : 0);
                command.Parameters.AddWithValue("@Priority", (int)task.Priority);
                command.Parameters.AddWithValue("@DueDate", ToDatabaseDate(task.DueDate));
                command.Parameters.AddWithValue("@Category", task.Category);
                command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt.ToString(DateFormat, CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@UpdatedAt", task.UpdatedAt.ToString(DateFormat, CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@CompletedAt", ToDatabaseDate(task.CompletedAt));
                command.ExecuteNonQuery();
            }
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static TodoTask ReadTask(SQLiteDataReader reader)
        {
            return new TodoTask
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                IsCompleted = reader.GetInt32(3) != 0,
                Priority = (TaskPriority)reader.GetInt32(4),
                DueDate = ParseDatabaseDate(reader.IsDBNull(5) ? null : reader.GetString(5)),
                Category = reader.GetString(6),
                CreatedAt = ParseDatabaseDate(reader.GetString(7)).GetValueOrDefault(),
                UpdatedAt = ParseDatabaseDate(reader.GetString(8)).GetValueOrDefault(),
                CompletedAt = ParseDatabaseDate(reader.IsDBNull(9) ? null : reader.GetString(9))
            };
        }

        private static object ToDatabaseDate(DateTime? value)
        {
            return value.HasValue ? (object)value.Value.ToString(DateFormat, CultureInfo.InvariantCulture) : DBNull.Value;
        }

        private static DateTime? ParseDatabaseDate(string value)
        {
            DateTime parsed;
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed) ? parsed : (DateTime?)null;
        }
    }
}
