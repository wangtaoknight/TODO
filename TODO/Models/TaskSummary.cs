namespace TODO.Models
{
    public sealed class TaskSummary
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Today { get; set; }
        public int Overdue { get; set; }
    }
}
