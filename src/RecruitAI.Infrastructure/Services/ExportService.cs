// RecruitAI.Infrastructure/Services/ExportService.cs
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data;
using System.ComponentModel;
using System.Text;

namespace RecruitAI.Infrastructure.Services;

public class ExportService : IExportService
{
	private readonly RecruitDevContext _context;
	private readonly IMessageService _msg;
	private readonly string _language;

	public ExportService(RecruitDevContext context, IMessageService messageService)
	{
		_context = context;
		_msg = messageService;
		ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

		// Lấy ngôn ngữ từ thread current culture
		_language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
	}

	public string GetContentType(string format)
	{
		return format?.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
	}

	public string GetFileExtension(string format)
	{
		return format?.ToLower() == "csv" ? "csv" : "xlsx";
	}

	public async Task<byte[]> ExportUsersAsync(ExportFilterDto filter, CancellationToken cancellationToken = default)
	{
		var users = await GetUsersForExport(filter, cancellationToken);
		return filter?.Format?.ToLower() == "csv"
			? ExportUsersToCsv(users)
			: await ExportUsersToExcelAsync(users);
	}

	public async Task<byte[]> ExportJobsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default)
	{
		var jobs = await GetJobsForExport(filter, cancellationToken);
		return filter?.Format?.ToLower() == "csv"
			? ExportJobsToCsv(jobs)
			: await ExportJobsToExcelAsync(jobs);
	}

	public async Task<byte[]> ExportApplicationsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default)
	{
		var applications = await GetApplicationsForExport(filter, cancellationToken);
		return filter?.Format?.ToLower() == "csv"
			? ExportApplicationsToCsv(applications)
			: await ExportApplicationsToExcelAsync(applications);
	}

	#region Users

	private async Task<List<User>> GetUsersForExport(ExportFilterDto filter, CancellationToken cancellationToken)
	{
		var query = _context.Users.AsQueryable();

		if (!string.IsNullOrEmpty(filter?.Role) && Enum.TryParse<UserRole>(filter.Role, true, out var role))
			query = query.Where(u => u.Role == role);

		if (filter?.Status.HasValue == true)
			query = query.Where(u => (int)u.Status == filter.Status.Value);

		if (filter?.FromDate.HasValue == true)
			query = query.Where(u => u.CreatedAt >= filter.FromDate.Value);

		if (filter?.ToDate.HasValue == true)
			query = query.Where(u => u.CreatedAt <= filter.ToDate.Value);

		if (!string.IsNullOrEmpty(filter?.Keyword))
		{
			var keyword = filter.Keyword.ToLower();
			query = query.Where(u => u.Email.ToLower().Contains(keyword) || u.FullName.ToLower().Contains(keyword));
		}

		return await query.OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);
	}

	private async Task<byte[]> ExportUsersToExcelAsync(List<User> users)
	{
		using var package = new ExcelPackage();
		var worksheet = package.Workbook.Worksheets.Add(_msg.Get("Export.Users.Title"));

		var headers = GetUserHeaders();
		for (int i = 0; i < headers.Length; i++)
			worksheet.Cells[1, i + 1].Value = headers[i];

		worksheet.Row(1).Style.Font.Bold = true;

		for (int i = 0; i < users.Count; i++)
		{
			var user = users[i];
			var row = i + 2;
			worksheet.Cells[row, 1].Value = user.Id.ToString();
			worksheet.Cells[row, 2].Value = user.Email;
			worksheet.Cells[row, 3].Value = user.FullName;
			worksheet.Cells[row, 4].Value = GetUserRoleDisplay(user.Role);
			worksheet.Cells[row, 5].Value = GetUserStatusDisplay(user.Status);
			worksheet.Cells[row, 6].Value = GetBooleanDisplay(user.EmailVerified);
			worksheet.Cells[row, 7].Value = user.PhoneNumber;
			worksheet.Cells[row, 8].Value = GetGenderDisplay(user.Gender);
			worksheet.Cells[row, 9].Value = user.DateOfBirth?.ToString("dd/MM/yyyy");
			worksheet.Cells[row, 10].Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm");
			worksheet.Cells[row, 11].Value = user.LastLoginAt?.ToString("dd/MM/yyyy HH:mm");
		}

		worksheet.Cells.AutoFitColumns();
		return await package.GetAsByteArrayAsync();
	}

	private byte[] ExportUsersToCsv(List<User> users)
	{
		using var memoryStream = new MemoryStream();
		using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
		using var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

		var headers = GetUserHeaders();
		foreach (var header in headers)
			csv.WriteField(header);
		csv.NextRecord();

		foreach (var user in users)
		{
			csv.WriteField(user.Id.ToString());
			csv.WriteField(user.Email);
			csv.WriteField(user.FullName);
			csv.WriteField(GetUserRoleDisplay(user.Role));
			csv.WriteField(GetUserStatusDisplay(user.Status));
			csv.WriteField(GetBooleanDisplay(user.EmailVerified));
			csv.WriteField(user.PhoneNumber);
			csv.WriteField(GetGenderDisplay(user.Gender));
			csv.WriteField(user.DateOfBirth?.ToString("dd/MM/yyyy"));
			csv.WriteField(user.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
			csv.WriteField(user.LastLoginAt?.ToString("dd/MM/yyyy HH:mm"));
			csv.NextRecord();
		}

		writer.Flush();
		return memoryStream.ToArray();
	}

	#endregion

	#region Jobs

	private async Task<List<Job>> GetJobsForExport(ExportFilterDto filter, CancellationToken cancellationToken)
	{
		var query = _context.Jobs
			.Include(j => j.Recruiter)
			.Include(j => j.Company)
			.AsQueryable();

		if (filter?.Status.HasValue == true)
			query = query.Where(j => (int)j.Status == filter.Status.Value);

		if (filter?.FromDate.HasValue == true)
			query = query.Where(j => j.CreatedAt >= filter.FromDate.Value);

		if (filter?.ToDate.HasValue == true)
			query = query.Where(j => j.CreatedAt <= filter.ToDate.Value);

		if (!string.IsNullOrEmpty(filter?.Keyword))
		{
			var keyword = filter.Keyword.ToLower();
			query = query.Where(j => j.Title.ToLower().Contains(keyword));
		}

		return await query.OrderByDescending(j => j.CreatedAt).ToListAsync(cancellationToken);
	}

	private async Task<byte[]> ExportJobsToExcelAsync(List<Job> jobs)
	{
		using var package = new ExcelPackage();
		var worksheet = package.Workbook.Worksheets.Add(_msg.Get("Export.Jobs.Title"));

		var headers = GetJobHeaders();
		for (int i = 0; i < headers.Length; i++)
			worksheet.Cells[1, i + 1].Value = headers[i];

		worksheet.Row(1).Style.Font.Bold = true;

		for (int i = 0; i < jobs.Count; i++)
		{
			var job = jobs[i];
			var row = i + 2;
			worksheet.Cells[row, 1].Value = job.Id.ToString();
			worksheet.Cells[row, 2].Value = job.Title;
			worksheet.Cells[row, 3].Value = job.Company?.Name;
			worksheet.Cells[row, 4].Value = job.Recruiter?.FullName;
			worksheet.Cells[row, 5].Value = job.Location;
			worksheet.Cells[row, 6].Value = job.SalaryMin;
			worksheet.Cells[row, 7].Value = job.SalaryMax;
			worksheet.Cells[row, 8].Value = GetEmploymentTypeDisplay(job.EmploymentType);
			worksheet.Cells[row, 9].Value = GetExperienceLevelDisplay(job.ExperienceLevel);
			worksheet.Cells[row, 10].Value = GetJobStatusDisplay(job.Status);
			worksheet.Cells[row, 11].Value = job.Views;
			worksheet.Cells[row, 12].Value = job.Applications;
			worksheet.Cells[row, 13].Value = job.CreatedAt.ToString("dd/MM/yyyy HH:mm");
			worksheet.Cells[row, 14].Value = job.ExpirationDate.ToString("dd/MM/yyyy");
		}

		worksheet.Cells.AutoFitColumns();
		return await package.GetAsByteArrayAsync();
	}

	private byte[] ExportJobsToCsv(List<Job> jobs)
	{
		using var memoryStream = new MemoryStream();
		using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
		using var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

		var headers = GetJobHeaders();
		foreach (var header in headers)
			csv.WriteField(header);
		csv.NextRecord();

		foreach (var job in jobs)
		{
			csv.WriteField(job.Id.ToString());
			csv.WriteField(job.Title);
			csv.WriteField(job.Company?.Name);
			csv.WriteField(job.Recruiter?.FullName);
			csv.WriteField(job.Location);
			csv.WriteField(job.SalaryMin);
			csv.WriteField(job.SalaryMax);
			csv.WriteField(GetEmploymentTypeDisplay(job.EmploymentType));
			csv.WriteField(GetExperienceLevelDisplay(job.ExperienceLevel));
			csv.WriteField(GetJobStatusDisplay(job.Status));
			csv.WriteField(job.Views);
			csv.WriteField(job.Applications);
			csv.WriteField(job.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
			csv.WriteField(job.ExpirationDate.ToString("dd/MM/yyyy"));
			csv.NextRecord();
		}

		writer.Flush();
		return memoryStream.ToArray();
	}

	#endregion

	#region Applications

	private async Task<List<JobApplication>> GetApplicationsForExport(ExportFilterDto filter, CancellationToken cancellationToken)
	{
		var query = _context.JobApplications
			.Include(a => a.Job)
			.Include(a => a.CV)
				.ThenInclude(cv => cv.User)
			.Include(a => a.Match)
			.AsQueryable();

		if (filter?.Status.HasValue == true)
			query = query.Where(a => (int)a.Status == filter.Status.Value);

		if (filter?.FromDate.HasValue == true)
			query = query.Where(a => a.AppliedAt >= filter.FromDate.Value);

		if (filter?.ToDate.HasValue == true)
			query = query.Where(a => a.AppliedAt <= filter.ToDate.Value);

		if (filter?.MinMatch.HasValue == true)
			query = query.Where(a => a.Match != null && a.Match.MatchPercentage >= filter.MinMatch.Value);

		return await query.OrderByDescending(a => a.AppliedAt).ToListAsync(cancellationToken);
	}

	private async Task<byte[]> ExportApplicationsToExcelAsync(List<JobApplication> applications)
	{
		using var package = new ExcelPackage();
		var worksheet = package.Workbook.Worksheets.Add(_msg.Get("Export.Applications.Title"));

		var headers = GetApplicationHeaders();
		for (int i = 0; i < headers.Length; i++)
			worksheet.Cells[1, i + 1].Value = headers[i];

		worksheet.Row(1).Style.Font.Bold = true;

		for (int i = 0; i < applications.Count; i++)
		{
			var app = applications[i];
			var row = i + 2;
			worksheet.Cells[row, 1].Value = app.Id.ToString();
			worksheet.Cells[row, 2].Value = app.Job?.Title;
			worksheet.Cells[row, 3].Value = app.CV?.User?.FullName;
			worksheet.Cells[row, 4].Value = app.CV?.User?.Email;
			worksheet.Cells[row, 5].Value = app.CV?.FileName;
			worksheet.Cells[row, 6].Value = GetApplicationStatusDisplay(app.Status);
			worksheet.Cells[row, 7].Value = app.Match?.MatchPercentage;
			worksheet.Cells[row, 8].Value = app.AppliedAt.ToString("dd/MM/yyyy HH:mm");
			worksheet.Cells[row, 9].Value = app.ReviewedAt?.ToString("dd/MM/yyyy HH:mm");
		}

		worksheet.Cells.AutoFitColumns();
		return await package.GetAsByteArrayAsync();
	}

	private byte[] ExportApplicationsToCsv(List<JobApplication> applications)
	{
		using var memoryStream = new MemoryStream();
		using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
		using var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

		var headers = GetApplicationHeaders();
		foreach (var header in headers)
			csv.WriteField(header);
		csv.NextRecord();

		foreach (var app in applications)
		{
			csv.WriteField(app.Id.ToString());
			csv.WriteField(app.Job?.Title);
			csv.WriteField(app.CV?.User?.FullName);
			csv.WriteField(app.CV?.User?.Email);
			csv.WriteField(app.CV?.FileName);
			csv.WriteField(GetApplicationStatusDisplay(app.Status));
			csv.WriteField(app.Match?.MatchPercentage);
			csv.WriteField(app.AppliedAt.ToString("dd/MM/yyyy HH:mm"));
			csv.WriteField(app.ReviewedAt?.ToString("dd/MM/yyyy HH:mm"));
			csv.NextRecord();
		}

		writer.Flush();
		return memoryStream.ToArray();
	}

	#endregion

	#region Headers

	private string[] GetUserHeaders()
	{
		return new[]
		{
			_msg.Get("Export.User.Id"),
			_msg.Get("Export.User.Email"),
			_msg.Get("Export.User.FullName"),
			_msg.Get("Export.User.Role"),
			_msg.Get("Export.User.Status"),
			_msg.Get("Export.User.EmailVerified"),
			_msg.Get("Export.User.PhoneNumber"),
			_msg.Get("Export.User.Gender"),
			_msg.Get("Export.User.DateOfBirth"),
			_msg.Get("Export.User.CreatedAt"),
			_msg.Get("Export.User.LastLoginAt")
		};
	}

	private string[] GetJobHeaders()
	{
		return new[]
		{
			_msg.Get("Export.Job.Id"),
			_msg.Get("Export.Job.Title"),
			_msg.Get("Export.Job.Company"),
			_msg.Get("Export.Job.Recruiter"),
			_msg.Get("Export.Job.Location"),
			_msg.Get("Export.Job.SalaryMin"),
			_msg.Get("Export.Job.SalaryMax"),
			_msg.Get("Export.Job.EmploymentType"),
			_msg.Get("Export.Job.ExperienceLevel"),
			_msg.Get("Export.Job.Status"),
			_msg.Get("Export.Job.Views"),
			_msg.Get("Export.Job.Applications"),
			_msg.Get("Export.Job.CreatedAt"),
			_msg.Get("Export.Job.ExpirationDate")
		};
	}

	private string[] GetApplicationHeaders()
	{
		return new[]
		{
			_msg.Get("Export.Application.Id"),
			_msg.Get("Export.Application.Job"),
			_msg.Get("Export.Application.Candidate"),
			_msg.Get("Export.Application.Email"),
			_msg.Get("Export.Application.CV"),
			_msg.Get("Export.Application.Status"),
			_msg.Get("Export.Application.MatchPercentage"),
			_msg.Get("Export.Application.AppliedAt"),
			_msg.Get("Export.Application.ReviewedAt")
		};
	}

	#endregion

	#region Display Helpers

	private string GetUserRoleDisplay(UserRole role) => role switch
	{
		UserRole.ADMIN => _msg.Get("UserRole.Admin"),
		UserRole.RECRUITER => _msg.Get("UserRole.Recruiter"),
		UserRole.CANDIDATE => _msg.Get("UserRole.Candidate"),
		_ => role.ToString()
	};

	private string GetUserStatusDisplay(UserStatus status) => status switch
	{
		UserStatus.Active => _msg.Get("UserStatus.Active"),
		UserStatus.Inactive => _msg.Get("UserStatus.Inactive"),
		UserStatus.Locked => _msg.Get("UserStatus.Locked"),
		UserStatus.PendingVerification => _msg.Get("UserStatus.PendingVerification"),
		UserStatus.Deleted => _msg.Get("UserStatus.Deleted"),
		UserStatus.Banned => _msg.Get("UserStatus.Banned"),
		_ => status.ToString()
	};

	private string GetGenderDisplay(Gender? gender) => gender switch
	{
		Gender.Male => _msg.Get("Gender.Male"),
		Gender.Female => _msg.Get("Gender.Female"),
		Gender.Other => _msg.Get("Gender.Other"),
		null => "",
		_ => gender.ToString()
	};

	private string GetBooleanDisplay(bool value) => value
		? _msg.Get("Common.Yes")
		: _msg.Get("Common.No");

	private string GetEmploymentTypeDisplay(EmploymentType? type) => type switch
	{
		EmploymentType.FullTime => _msg.Get("EmploymentType.FullTime"),
		EmploymentType.PartTime => _msg.Get("EmploymentType.PartTime"),
		EmploymentType.Remote => _msg.Get("EmploymentType.Remote"),
		EmploymentType.Hybrid => _msg.Get("EmploymentType.Hybrid"),
		EmploymentType.Contract => _msg.Get("EmploymentType.Contract"),
		EmploymentType.Internship => _msg.Get("EmploymentType.Internship"),
		null => "",
		_ => type.ToString()
	};

	private string GetExperienceLevelDisplay(ExperienceLevel? level) => level switch
	{
		ExperienceLevel.Entry => _msg.Get("ExperienceLevel.Entry"),
		ExperienceLevel.Junior => _msg.Get("ExperienceLevel.Junior"),
		ExperienceLevel.Intermediate => _msg.Get("ExperienceLevel.Intermediate"),
		ExperienceLevel.Senior => _msg.Get("ExperienceLevel.Senior"),
		ExperienceLevel.Lead => _msg.Get("ExperienceLevel.Lead"),
		ExperienceLevel.Manager => _msg.Get("ExperienceLevel.Manager"),
		_ => level?.ToString() ?? ""
	};

	private string GetJobStatusDisplay(JobStatus status) => status switch
	{
		JobStatus.Draft => _msg.Get("JobStatus.Draft"),
		JobStatus.Published => _msg.Get("JobStatus.Published"),
		JobStatus.Closed => _msg.Get("JobStatus.Closed"),
		JobStatus.Expired => _msg.Get("JobStatus.Expired"),
		JobStatus.Pending => _msg.Get("JobStatus.Pending"),
		_ => status.ToString()
	};

	private string GetApplicationStatusDisplay(JobApplicationStatus status) => status switch
	{
		JobApplicationStatus.Pending => _msg.Get("ApplicationStatus.Pending"),
		JobApplicationStatus.Reviewed => _msg.Get("ApplicationStatus.Reviewed"),
		JobApplicationStatus.Accepted => _msg.Get("ApplicationStatus.Accepted"),
		JobApplicationStatus.Rejected => _msg.Get("ApplicationStatus.Rejected"),
		_ => status.ToString()
	};

	#endregion
}