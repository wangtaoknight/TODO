using System;

namespace TODO.Services
{
    public sealed class TaskServiceException : Exception
    {
        public TaskServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
