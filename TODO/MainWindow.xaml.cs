using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TODO.Models;
using TODO.Services;

namespace TODO
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TodoTaskService _service;
        private readonly ObservableCollection<TodoTask> _visibleTasks = new ObservableCollection<TodoTask>();
        private TaskFilter _filter = TaskFilter.All;
        private TaskSortOption _sort = TaskSortOption.CreatedDescending;

        public MainWindow(TodoTaskService service)
        {
            _service = service ?? throw new ArgumentNullException("service");
            InitializeComponent();
            TaskList.ItemsSource = _visibleTasks;
            SortBox.Items.Add(new ComboBoxItem { Content = "最近创建", Tag = TaskSortOption.CreatedDescending });
            SortBox.Items.Add(new ComboBoxItem { Content = "截止日期", Tag = TaskSortOption.DueDateAscending });
            SortBox.Items.Add(new ComboBoxItem { Content = "优先级", Tag = TaskSortOption.PriorityDescending });
            SortBox.Items.Add(new ComboBoxItem { Content = "标题 A-Z", Tag = TaskSortOption.TitleAscending });
            SortBox.SelectedIndex = 0;
            RefreshTasks();
        }

        private void RefreshTasks()
        {
            try
            {
                var tasks = _service.GetTasks(SearchBox == null ? string.Empty : SearchBox.Text, _filter, _sort);
                _visibleTasks.Clear();
                foreach (var task in tasks)
                {
                    _visibleTasks.Add(task);
                }

                var summary = _service.GetSummary();
                TotalText.Text = "全部任务       " + summary.Total;
                ActiveText.Text = "进行中         " + summary.Active;
                CompletedText.Text = "已完成         " + summary.Completed;
                OverdueText.Text = "已逾期         " + summary.Overdue;
                EmptyText.Visibility = _visibleTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                MessageBox.Show("加载任务失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            _filter = (TaskFilter)Enum.Parse(typeof(TaskFilter), (string)button.Tag);
            FilterTitle.Text = button.Content.ToString();
            RefreshTasks();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized) RefreshTasks();
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = SortBox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                _sort = (TaskSortOption)item.Tag;
                RefreshTasks();
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskEditWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _service.Create(dialog.TaskTitle, dialog.TaskDescription, dialog.TaskPriority, dialog.TaskDueDate, dialog.TaskCategory);
                    RefreshTasks();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("保存任务失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var task = (TodoTask)button.Tag;
            var dialog = new TaskEditWindow(task) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    task.Title = dialog.TaskTitle;
                    task.Description = dialog.TaskDescription;
                    task.Priority = dialog.TaskPriority;
                    task.DueDate = dialog.TaskDueDate;
                    task.Category = dialog.TaskCategory;
                    _service.Update(task);
                    RefreshTasks();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("更新任务失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var task = (TodoTask)((Button)sender).Tag;
            if (MessageBox.Show("确定要删除“" + task.Title + "”吗？", "删除任务", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _service.Delete(task);
                RefreshTasks();
            }
            catch (Exception exception)
            {
                MessageBox.Show("删除任务失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskCompleted_Click(object sender, RoutedEventArgs e)
        {
            var task = (TodoTask)((CheckBox)sender).DataContext;
            try
            {
                _service.Update(task);
                RefreshTasks();
            }
            catch (Exception exception)
            {
                MessageBox.Show("更新完成状态失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshTasks();
            }
        }
    }
}
