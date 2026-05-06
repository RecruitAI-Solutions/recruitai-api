using RecruitAI.Domain.Enums;

namespace RecruitAI.Shared.Interfaces
{
	public interface IMessageService
	{
		string Get(string key, params object[] args);
		string Business(string key, params object[] args);
		string Log(string key, params object[] args);
		string Validation(string key, params object[] args);
		string Success(string key, params object[] args);
		string GetSalaryRangeDisplay(decimal? salaryMin, decimal? salaryMax);

		// New methods
		string GetApplicationStatusDisplay(JobApplicationStatus status);
		string GetCVStatusDisplay(CVStatus status);
		string GetUserRoleDisplay(UserRole role);
		string GetJobStatusDisplay(JobStatus status);
		string FormatFileSize(long bytes);
		string GetRelativeTime(DateTime dateTime);
	}
}