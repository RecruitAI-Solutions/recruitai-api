using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System.Text.Json;

namespace RecruitAI.Application.Helpers
{
	public static class AuditLogHelper
	{
		public static string GetEntityTypeName(AuditEntityType entityType, IMessageService msg)
		{
			return entityType switch
			{
				AuditEntityType.User => msg.Get("AuditEntityTypeUser"),
				AuditEntityType.CV => msg.Get("AuditEntityTypeCV"),
				AuditEntityType.Job => msg.Get("AuditEntityTypeJob"),
				AuditEntityType.Application => msg.Get("AuditEntityTypeApplication"),
				_ => entityType.ToString()
			};
		}

		public static string GetActionName(AuditAction action, IMessageService msg)
		{
			return action switch
			{
				AuditAction.Create => msg.Get("AuditActionCreate"),
				AuditAction.Update => msg.Get("AuditActionUpdate"),
				AuditAction.Delete => msg.Get("AuditActionDelete"),
				AuditAction.Login => msg.Get("AuditActionLogin"),
				AuditAction.Logout => msg.Get("AuditActionLogout"),
				AuditAction.RefreshToken => msg.Get("AuditActionRefreshToken"),
				AuditAction.ChangePassword => msg.Get("AuditActionChangePassword"),
				AuditAction.ChangeStatus => msg.Get("AuditActionChangeStatus"),
				AuditAction.ChangeRole => msg.Get("AuditActionChangeRole"),
				AuditAction.Upload => msg.Get("AuditActionUpload"),
				AuditAction.Analyze => msg.Get("AuditActionAnalyze"),
				AuditAction.CreateJob => msg.Get("AuditActionCreateJob"),
				AuditAction.UpdateJob => msg.Get("AuditActionUpdateJob"),
				AuditAction.DeleteJob => msg.Get("AuditActionDeleteJob"),
				AuditAction.Apply => msg.Get("AuditActionApply"),
				AuditAction.UpdateStatus => msg.Get("AuditActionUpdateStatus"),
				_ => action.ToString()
			};
		}
		public static string FormatData(Dictionary<string, string> data, IMessageService msg)
		{
			return JsonSerializer.Serialize(data);
		}
		public static string FormatUserData(User user, IMessageService msg)
		{
			var data = new Dictionary<string, string>();
			data[msg.Get("AuditFieldEmail")] = user.Email;
			data[msg.Get("AuditFieldRole")] = user.Role.ToString();
			if (!string.IsNullOrEmpty(user.FullName))
				data[msg.Get("AuditFieldFullName")] = user.FullName;
			if (!string.IsNullOrEmpty(user.PhoneNumber))
				data[msg.Get("AuditFieldPhoneNumber")] = user.PhoneNumber;
			if (user.Gender.HasValue)
				data[msg.Get("AuditFieldGender")] = user.Gender.ToString();
			if (user.DateOfBirth.HasValue)
				data[msg.Get("AuditFieldDateOfBirth")] = user.DateOfBirth.Value.ToShortDateString();
			return JsonSerializer.Serialize(data);
		}

		public static string FormatJobData(Job job, IMessageService msg)
		{
			var data = new Dictionary<string, string>();
			data[msg.Get("AuditFieldTitle")] = job.Title;
			data[msg.Get("AuditFieldLocation")] = job.Location;
			if (job.SalaryMin.HasValue)
				data[msg.Get("AuditFieldSalaryMin")] = job.SalaryMin.Value.ToString("N0");
			if (job.SalaryMax.HasValue)
				data[msg.Get("AuditFieldSalaryMax")] = job.SalaryMax.Value.ToString("N0");
			if (!string.IsNullOrEmpty(job.Department))
				data[msg.Get("AuditFieldDepartment")] = job.Department;
			return JsonSerializer.Serialize(data);
		}

	}
}