using CybersecurityAwarenessBot.Gui.Models;
using MySqlConnector;

namespace CybersecurityAwarenessBot.Gui.Services;

/// <summary>
/// Handles all database operations for cybersecurity tasks.
/// Uses MySQL when available; falls back to in-memory storage automatically
/// so the application still works without a database configuration.
/// </summary>
public sealed class DatabaseService : IDisposable
{
    // ── Configuration ───────────────────────────────────────────────────────
    // TODO: Update this connection string to match your MySQL installation.
    // Default assumes MySQL running locally with an empty root password.
    public const string DefaultConnectionString =
        "Server=localhost;Database=cybersecurity_bot;Uid=root;Pwd=;";

    private readonly string _connectionString;

    // ── Runtime state ───────────────────────────────────────────────────────
    private bool _mysqlAvailable;

    // In-memory fallback (used when MySQL is not reachable)
    private readonly List<CyberTask> _memTasks = new();
    private int _nextMemId = 1;

    // ── Constructor ─────────────────────────────────────────────────────────

    public DatabaseService(string? connectionString = null)
    {
        _connectionString = connectionString ?? DefaultConnectionString;
    }

    // ── Public status ───────────────────────────────────────────────────────

    /// <summary>True if the app connected to MySQL; false if using in-memory fallback.</summary>
    public bool IsUsingDatabase => _mysqlAvailable;

    // ── Initialisation ──────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to connect to MySQL and create the tasks table.
    /// Silently falls back to in-memory mode on any error.
    /// </summary>
    public void Initialise()
    {
        try
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS cyber_tasks (
                    Id           INT AUTO_INCREMENT PRIMARY KEY,
                    Title        VARCHAR(200)  NOT NULL,
                    Description  TEXT          NOT NULL DEFAULT '',
                    ReminderDate DATETIME      NULL,
                    IsCompleted  TINYINT(1)    NOT NULL DEFAULT 0,
                    CreatedAt    DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";
            cmd.ExecuteNonQuery();
            _mysqlAvailable = true;
        }
        catch
        {
            _mysqlAvailable = false;
        }
    }

    // ── CRUD Operations ─────────────────────────────────────────────────────

    /// <summary>Returns all tasks ordered by creation date (newest first).</summary>
    public List<CyberTask> GetAllTasks()
    {
        if (!_mysqlAvailable)
            return new List<CyberTask>(_memTasks.OrderByDescending(t => t.CreatedAt));

        var tasks = new List<CyberTask>();
        try
        {
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(
                "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt " +
                "FROM cyber_tasks ORDER BY CreatedAt DESC;", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                tasks.Add(new CyberTask
                {
                    Id           = rdr.GetInt32(0),
                    Title        = rdr.GetString(1),
                    Description  = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                    ReminderDate = rdr.IsDBNull(3) ? null : rdr.GetDateTime(3),
                    IsCompleted  = rdr.GetBoolean(4),
                    CreatedAt    = rdr.GetDateTime(5)
                });
            }
        }
        catch { /* return whatever was collected */ }
        return tasks;
    }

    /// <summary>
    /// Inserts a new task and returns its generated Id.
    /// Returns -1 on failure.
    /// </summary>
    public int AddTask(CyberTask task)
    {
        if (!_mysqlAvailable)
        {
            task.Id        = _nextMemId++;
            task.CreatedAt = DateTime.Now;
            _memTasks.Add(task);
            return task.Id;
        }

        try
        {
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(
                "INSERT INTO cyber_tasks (Title, Description, ReminderDate, IsCompleted) " +
                "VALUES (@t, @d, @r, 0); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@t", task.Title);
            cmd.Parameters.AddWithValue("@d", task.Description);
            cmd.Parameters.AddWithValue("@r", (object?)task.ReminderDate ?? DBNull.Value);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            task.Id = id;
            return id;
        }
        catch { return -1; }
    }

    /// <summary>Sets or updates the reminder date for a task.</summary>
    public void SetReminder(int taskId, DateTime reminderDate)
    {
        if (!_mysqlAvailable)
        {
            var t = _memTasks.FirstOrDefault(x => x.Id == taskId);
            if (t != null) t.ReminderDate = reminderDate;
            return;
        }

        try
        {
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(
                "UPDATE cyber_tasks SET ReminderDate = @r WHERE Id = @id;", conn);
            cmd.Parameters.AddWithValue("@r", reminderDate);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    /// <summary>Marks a task as completed (or un-marks it).</summary>
    public void CompleteTask(int taskId, bool completed = true)
    {
        if (!_mysqlAvailable)
        {
            var t = _memTasks.FirstOrDefault(x => x.Id == taskId);
            if (t != null) t.IsCompleted = completed;
            return;
        }

        try
        {
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(
                "UPDATE cyber_tasks SET IsCompleted = @c WHERE Id = @id;", conn);
            cmd.Parameters.AddWithValue("@c", completed ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    /// <summary>Permanently deletes a task by its Id.</summary>
    public void DeleteTask(int taskId)
    {
        if (!_mysqlAvailable)
        {
            _memTasks.RemoveAll(x => x.Id == taskId);
            return;
        }

        try
        {
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(
                "DELETE FROM cyber_tasks WHERE Id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private MySqlConnection OpenConnection()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void Dispose() { }
}
