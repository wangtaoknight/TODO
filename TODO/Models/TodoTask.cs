using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TODO.Models
{
    public sealed class TodoTask : INotifyPropertyChanged
    {
        private Guid _id;
        private string _title;
        private string _description;
        private bool _isCompleted;
        private TaskPriority _priority;
        private DateTime? _dueDate;
        private string _category;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private DateTime? _completedAt;

        public TodoTask()
        {
            _id = Guid.NewGuid();
            _title = string.Empty;
            _description = string.Empty;
            _category = "默认";
            _priority = TaskPriority.Medium;
            _createdAt = DateTime.Now;
            _updatedAt = _createdAt;
        }

        public Guid Id
        {
            get { return _id; }
            set { SetField(ref _id, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value ?? string.Empty); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value ?? string.Empty); }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                if (SetField(ref _isCompleted, value))
                {
                    CompletedAt = value ? DateTime.Now : (DateTime?)null;
                }
            }
        }

        public TaskPriority Priority
        {
            get { return _priority; }
            set { SetField(ref _priority, value); }
        }

        public DateTime? DueDate
        {
            get { return _dueDate; }
            set { SetField(ref _dueDate, value); }
        }

        public string Category
        {
            get { return _category; }
            set { SetField(ref _category, string.IsNullOrWhiteSpace(value) ? "默认" : value.Trim()); }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { SetField(ref _createdAt, value); }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { SetField(ref _updatedAt, value); }
        }

        public DateTime? CompletedAt
        {
            get { return _completedAt; }
            set { SetField(ref _completedAt, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
