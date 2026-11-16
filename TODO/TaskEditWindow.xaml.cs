using System;
using System.Windows;
using TODO.Models;

namespace TODO
{
    public partial class TaskEditWindow : Window
    {
        private readonly TodoTask _originalTask;

        public TaskEditWindow(TodoTask task = null)
        {
            InitializeComponent();
            _originalTask = task;
            HeaderText.Text = task == null ? "新建任务" : "编辑任务";
            PriorityBox.ItemsSource = Enum.GetValues(typeof(TaskPriority));
            PriorityBox.SelectedItem = task == null ? (object)TaskPriority.Medium : task.Priority;
            TitleBox.Text = task == null ? string.Empty : task.Title;
            DescriptionBox.Text = task == null ? string.Empty : task.Description;
            CategoryBox.Text = task == null ? "默认" : task.Category;
            DueDateBox.SelectedDate = task == null ? (DateTime?)null : task.DueDate;
        }

        public string TaskTitle { get { return TitleBox.Text; } }
        public string TaskDescription { get { return DescriptionBox.Text; } }
        public TaskPriority TaskPriority { get { return (TaskPriority)PriorityBox.SelectedItem; } }
        public DateTime? TaskDueDate { get { return DueDateBox.SelectedDate; } }
        public string TaskCategory { get { return CategoryBox.Text; } }
        public TodoTask OriginalTask { get { return _originalTask; } }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                MessageBox.Show("请输入任务标题。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                TitleBox.Focus();
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
