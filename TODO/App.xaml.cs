using System.Windows;
using TODO.Data;
using TODO.Services;

namespace TODO
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var repository = new SqliteTaskRepository();
                var window = new MainWindow(new TodoTaskService(repository));
                MainWindow = window;
                window.Show();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("应用初始化失败：" + exception.Message, "TODO", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }
    }
}
