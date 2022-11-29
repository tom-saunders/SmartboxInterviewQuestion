using System;
using System.Runtime.Serialization;

namespace InterviewQuestion
{
    [Serializable]
    public class SeeTechStartException : InvalidOperationException
    {
        public SeeTechStartException()
        {
        }

        public SeeTechStartException(string? message) : base(message)
        {
        }

        public SeeTechStartException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SeeTechStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}