using System;
using System.Collections.Generic;
using System.Linq;
using TODO.Data;
using TODO.Models;

namespace TODO.Services
{
    public sealed class TodoTaskService
    {
        private readonly SqliteTaskRepository _repository;

        public TodoTaskService(SqliteTaskRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException("repository");
        }

        public IList<TodoTask> GetTasks(string searchText, TaskFilter filter, TaskSortOption sort)
        {
            IEnumerable<TodoTask> query = _repository.GetAll();
            var normalizedSearch = (searchText ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(normalizedSearch))
            {
                query = query.Where(task => task.Title.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0
                    || task.Description.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0
                    || task.Category.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var today = DateTime.Today;
            switch (filter)
            {
                case TaskFilter.Active:
                    query = query.Where(task => !task.IsCompleted);
                    break;
                case TaskFilter.Completed:
                    query = query.Where(task => task.IsCompleted);
                    break;
                case TaskFilter.Today:
                    query = query.Where(task => task.DueDate.HasValue && task.DueDate.Value.Date == today);
                    break;
                case TaskFilter.Overdue:
                    query = query.Where(task => !task.IsCompleted && task.DueDate.HasValue && task.DueDate.Value.Date < today);
                    break;
            }

            switch (sort)
            {
                case TaskSortOption.DueDateAscending:
                    query = query.OrderBy(task => task.DueDate.HasValue ? 0 : 1).ThenBy(task => task.DueDate).ThenByDescending(task => task.CreatedAt);
                    break;
                case TaskSortOption.PriorityDescending:
                    query = query.OrderByDescending(task => task.Priority).ThenBy(task => task.IsCompleted).ThenByDescending(task => task.CreatedAt);
                    break;
                case TaskSortOption.TitleAscending:
                    query = query.OrderBy(task => task.Title).ThenByDescending(task => task.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(task => task.IsCompleted).ThenByDescending(task => task.CreatedAt);
                    break;
            }

            return query.ToList();
        }

        public TaskSummary GetSummary()
        {
            var tasks = _repository.GetAll();
            var today = DateTime.Today;
            return new TaskSummary
            {
                Total = tasks.Count,
                Active = tasks.Count(task => !task.IsCompleted),
                Completed = tasks.Count(task => task.IsCompleted),
                Today = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value.Date == today),
                Overdue = tasks.Count(task => !task.IsCompleted && task.DueDate.HasValue && task.DueDate.Value.Date < today)
            };
        }

        public TodoTask Create(string title, string description, TaskPriority priority, DateTime? dueDate, string category)
        {
            var task = new TodoTask
            {
                Title = ValidateTitle(title),
                Description = description ?? string.Empty,
                Priority = priority,
                DueDate = dueDate,
                Category = category
            };
            _repository.Insert(task);
            return task;
        }

        public void Update(TodoTask task)
        {
            ValidateTask(task);
            task.Title = ValidateTitle(task.Title);
            task.UpdatedAt = DateTime.Now;
            _repository.Update(task);
        }

        public void ToggleCompleted(TodoTask task)
        {
            ValidateTask(task);
            task.IsCompleted = !task.IsCompleted;
            task.UpdatedAt = DateTime.Now;
            _repository.Update(task);
        }

        public void Delete(TodoTask task)
        {
            ValidateTask(task);
            _repository.Delete(task.Id);
        }

        private static string ValidateTitle(string title)
        {
            var normalized = (title ?? string.Empty).Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException("任务标题不能为空。", "title");
            }
            if (normalized.Length > 200)
            {
                throw new ArgumentException("任务标题不能超过 200 个字符。", "title");
            }
            return normalized;
        }

        private static void ValidateTask(TodoTask task)
        {
            if (task == null) throw new ArgumentNullException("task");
        }
    }
}
