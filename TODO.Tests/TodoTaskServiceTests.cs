using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TODO.Data;
using TODO.Models;
using TODO.Services;

namespace TODO.Tests
{
    [TestClass]
    public class TodoTaskServiceTests
    {
        private string _directory;
        private TodoTaskService _service;

        [TestInitialize]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "TODO.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _service = new TodoTaskService(new SqliteTaskRepository(Path.Combine(_directory, "tasks.db")));
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestMethod]
        public void CreateAndReload_PersistsTask()
        {
            var task = _service.Create("完成报告", "整理本周数据", TaskPriority.High, DateTime.Today, "工作");
            var tasks = _service.GetTasks(string.Empty, TaskFilter.All, TaskSortOption.CreatedDescending);

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task.Id, tasks[0].Id);
            Assert.AreEqual("工作", tasks[0].Category);
            Assert.AreEqual(TaskPriority.High, tasks[0].Priority);
        }

        [TestMethod]
        public void ToggleCompleted_UpdatesSummaryAndFilter()
        {
            var task = _service.Create("阅读", string.Empty, TaskPriority.Low, null, "学习");
            _service.ToggleCompleted(task);

            var summary = _service.GetSummary();
            Assert.AreEqual(0, summary.Active);
            Assert.AreEqual(1, summary.Completed);
            Assert.AreEqual(1, _service.GetTasks(string.Empty, TaskFilter.Completed, TaskSortOption.CreatedDescending).Count);
        }

        [TestMethod]
        public void SearchAndOverdueFilter_ReturnsExpectedTasks()
        {
            _service.Create("修复登录问题", "客户端", TaskPriority.High, DateTime.Today.AddDays(-1), "工作");
            _service.Create("购买牛奶", string.Empty, TaskPriority.Medium, DateTime.Today, "生活");

            Assert.AreEqual(1, _service.GetTasks("登录", TaskFilter.All, TaskSortOption.CreatedDescending).Count);
            Assert.AreEqual(1, _service.GetTasks(string.Empty, TaskFilter.Overdue, TaskSortOption.CreatedDescending).Count);
        }

        [TestMethod]
        public void Create_RejectsEmptyTitle()
        {
            Assert.ThrowsException<ArgumentException>(() => _service.Create("  ", string.Empty, TaskPriority.Medium, null, "默认"));
        }
    }
}
