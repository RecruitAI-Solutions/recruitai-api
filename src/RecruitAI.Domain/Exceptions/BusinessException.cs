// RecruitAI.Domain/Exceptions/BusinessException.cs
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Extensions;

namespace RecruitAI.Domain.Exceptions
{
	public class BusinessException : Exception
	{
		public ErrorCode ErrorCode { get; set; }
		public int StatusCode => ErrorCode.GetStatusCode();

		public BusinessException(ErrorCode errorCode, string message)
			: base(message)
		{
			ErrorCode = errorCode;
		}

		public BusinessException(ErrorCode errorCode, string message, Exception innerException)
			: base(message, innerException)
		{
			ErrorCode = errorCode;
		}
	}
}