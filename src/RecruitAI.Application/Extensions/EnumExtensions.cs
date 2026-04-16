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
	public static string GetDisplayName(this UserStatus status, IMessageService msg)
	{
		return status switch
		{
			UserStatus.Active => msg.Get("UserStatus.Active"),
			UserStatus.Inactive => msg.Get("UserStatus.Inactive"),
			UserStatus.Locked => msg.Get("UserStatus.Locked"),
			UserStatus.PendingVerification => msg.Get("UserStatus.PendingVerification"),
			UserStatus.Deleted => msg.Get("UserStatus.Deleted"),
			UserStatus.Banned => msg.Get("UserStatus.Banned"),
			_ => msg.Get("UserStatus.Updated")
		};
	}

	public static string GetDisplayName(this UserRole role, IMessageService msg)
	{
		return role switch
		{
			UserRole.CANDIDATE => msg.Get("UserRole.CANDIDATE"),
			UserRole.RECRUITER => msg.Get("UserRole.RECRUITER"),
			UserRole.ADMIN => msg.Get("UserRole.ADMIN"),
			_ => msg.Get("UserRole.Updated")
		};
	}
}