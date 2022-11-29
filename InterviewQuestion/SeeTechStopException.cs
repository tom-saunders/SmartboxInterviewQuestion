using System;
using System.Runtime.Serialization;

namespace InterviewQuestion
{
    [Serializable]
    public class SeeTechStopException : InvalidOperationException
    {
        public SeeTechStopException()
        {
        }

        public SeeTechStopException(string? message) : base(message)
        {
        }

        public SeeTechStopException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SeeTechStopException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}