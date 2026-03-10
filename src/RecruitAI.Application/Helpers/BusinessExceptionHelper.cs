using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Helpers
{
	public static class BusinessExceptionHelper
	{
		/// <summary>
		/// Throw BusinessException với ErrorCode và message key
		/// </summary>
		public static void Throw(this IMessageService msg, ErrorCode errorCode, string messageKey)
		{
			throw new BusinessException(errorCode, msg.Business(messageKey));
		}

		/// <summary>
		/// Throw BusinessException với ErrorCode, message key và tham số
		/// </summary>
		public static void Throw(this IMessageService msg, ErrorCode errorCode, string messageKey, params object[] args)
		{
			throw new BusinessException(errorCode, msg.Business(messageKey, args));
		}

		/// <summary>
		/// Throw BusinessException với ErrorCode, message key và inner exception
		/// </summary>
		public static void Throw(this IMessageService msg, ErrorCode errorCode, string messageKey, Exception innerException)
		{
			throw new BusinessException(errorCode, msg.Business(messageKey), innerException);
		}

		/// <summary>
		/// Throw BusinessException với ErrorCode, message key, tham số và inner exception
		/// </summary>
		public static void Throw(this IMessageService msg, ErrorCode errorCode, string messageKey, Exception innerException, params object[] args)
		{
			throw new BusinessException(errorCode, msg.Business(messageKey, args), innerException);
		}
	}
}