using System.Runtime.Serialization;

namespace RecruitAI.Domain.Exceptions
{
    [Serializable]
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
        protected DomainException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class TestValidationException : DomainException
    {
        public TestValidationException() { }
        public TestValidationException(string field) : base($"Validation failed for field: {field}") { }
        public TestValidationException(string message, Exception inner) : base(message, inner) { }
        protected TestValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}