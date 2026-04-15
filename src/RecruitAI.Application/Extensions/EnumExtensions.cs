using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Extensions;

public static class EnumExtensions
{
	public static string GetDisplayName(this JobApplicationStatus status, IMessageService msg)
	{
		return status switch
		{
			JobApplicationStatus.Accepted => msg.Get("ApplicationStatus.Accepted"),
			JobApplicationStatus.Rejected => msg.Get("ApplicationStatus.Rejected"),
			JobApplicationStatus.Reviewed => msg.Get("ApplicationStatus.Reviewed"),
			JobApplicationStatus.Pending => msg.Get("ApplicationStatus.Pending"),
			_ => msg.Get("ApplicationStatus.Updated")
		};
	}
}