using Microsoft.Extensions.Localization;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Resources;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Helpers
{
	public class MessageService : IMessageService
	{
		private readonly IStringLocalizer<Messages> _localizer;

		public MessageService(IStringLocalizer<Messages> localizer)
		{
			_localizer = localizer;
		}

		public string Get(string key, params object[] args)
		{
			var message = _localizer[key];
			return args?.Length > 0 ? string.Format(message, args) : message;
		}

		public string Business(string key, params object[] args) =>
			Get($"Business{key}", args);

		public string Log(string key, params object[] args) =>
			Get($"Log{key}", args);

		public string Validation(string key, params object[] args) =>
			Get($"Validation{key}", args);

		public string Success(string key, params object[] args) =>
			Get($"Success{key}", args);

		public string GetSalaryRangeDisplay(decimal? salaryMin, decimal? salaryMax)
		{
			if (salaryMin.HasValue && salaryMax.HasValue)
			{
				return Get("SalaryRangeFull",
					salaryMin.Value.ToString("N0"),
					salaryMax.Value.ToString("N0"));
			}

			if (salaryMin.HasValue)
			{
				return Get("SalaryRangeFrom", salaryMin.Value.ToString("N0"));
			}

			if (salaryMax.HasValue)
			{
				return Get("SalaryRangeTo", salaryMax.Value.ToString("N0"));
			}

			return Get("SalaryRangeNegotiable");
		}

		/// <summary>
		/// Lấy tên hiển thị của Application Status
		/// </summary>
		public string GetApplicationStatusDisplay(JobApplicationStatus status)
		{
			return status switch
			{
				JobApplicationStatus.Pending => Get("ApplicationStatus.Pending"),
				JobApplicationStatus.Reviewed => Get("ApplicationStatus.Reviewed"),
				JobApplicationStatus.Accepted => Get("ApplicationStatus.Accepted"),
				JobApplicationStatus.Rejected => Get("ApplicationStatus.Rejected"),
				_ => status.ToString()
			};
		}

		/// <summary>
		/// Lấy tên hiển thị của CV Status
		/// </summary>
		public string GetCVStatusDisplay(CVStatus status)
		{
			return status switch
			{
				CVStatus.Pending => Get("CVStatus.Pending"),
				CVStatus.Uploaded => Get("CVStatus.Uploaded"),
				CVStatus.Processing => Get("CVStatus.Processing"),
				CVStatus.Completed => Get("CVStatus.Completed"),
				CVStatus.Analyzed => Get("CVStatus.Analyzed"),
				CVStatus.Failed => Get("CVStatus.Failed"),
				_ => status.ToString()
			};
		}

		/// <summary>
		/// Lấy tên hiển thị của User Role
		/// </summary>
		public string GetUserRoleDisplay(UserRole role)
		{
			return role switch
			{
				UserRole.ADMIN => Get("UserRole.Admin"),
				UserRole.RECRUITER => Get("UserRole.Recruiter"),
				UserRole.CANDIDATE => Get("UserRole.Candidate"),
				_ => role.ToString()
			};
		}

		/// <summary>
		/// Format file size (bytes -> KB/MB)
		/// </summary>
		public string FormatFileSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:0.##} {sizes[order]}";
		}

		/// <summary>
		/// Format thời gian tương đối (vd: "2 giờ trước")
		/// </summary>
		public string GetRelativeTime(DateTime dateTime)
		{
			var diff = DateTime.UtcNow - dateTime;

			if (diff.TotalSeconds < 60)
				return Get("Time.JustNow");

			if (diff.TotalMinutes < 60)
				return string.Format(Get("Time.MinutesAgo"), (int)diff.TotalMinutes);

			if (diff.TotalHours < 24)
				return string.Format(Get("Time.HoursAgo"), (int)diff.TotalHours);

			if (diff.TotalDays < 7)
				return string.Format(Get("Time.DaysAgo"), (int)diff.TotalDays);

			if (diff.TotalDays < 30)
				return string.Format(Get("Time.WeeksAgo"), (int)(diff.TotalDays / 7));

			if (diff.TotalDays < 365)
				return string.Format(Get("Time.MonthsAgo"), (int)(diff.TotalDays / 30));

			return string.Format(Get("Time.YearsAgo"), (int)(diff.TotalDays / 365));
		}
	}
}