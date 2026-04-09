using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Jobs;

public class UpdateJobCommand : IRequest<JobDetailDto>
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Requirements { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;

	// Lương
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public Currency Currency { get; set; } = Currency.VND;

	// Loại hình
	public EmploymentType EmploymentType { get; set; }
	public ExperienceLevel ExperienceLevel { get; set; }
	public string Department { get; set; } = string.Empty;

	// Kỹ năng
	public List<int> SkillIds { get; set; } = new();
	public string Benefits { get; set; } = string.Empty;

	// Thời gian
	public DateTime ExpirationDate { get; set; }

	// Kiểm tra quyền sở hữu (sẽ được set từ Controller)
	public Guid RecruiterId { get; set; }
}

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, JobDetailDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<UpdateJobCommandHandler> _logger;
	private readonly IAuditLogService _auditLogService;  

	public UpdateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<UpdateJobCommandHandler> logger,
		IAuditLogService auditLogService)  
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
		_auditLogService = auditLogService;
	}

	public async Task<JobDetailDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Updating job {JobId} for recruiter {RecruiterId}",
				request.Id, request.RecruiterId);

			// Kiểm tra job tồn tại
			var existingJob = await _uow.Jobs.GetByIdAsync(request.Id, cancellationToken);
			if (existingJob == null)
			{
				throw new BusinessException(
					ErrorCode.ResourceNotFound,
					"Không tìm thấy công việc");
			}

			// Kiểm tra quyền sở hữu
			if (existingJob.RecruiterId != request.RecruiterId)
			{
				throw new BusinessException(
					ErrorCode.Forbidden,
					"Bạn không có quyền cập nhật công việc này");
			}

			// Lưu giá trị cũ
			var oldTitle = existingJob.Title;
			var oldLocation = existingJob.Location;
			var oldSalaryMin = existingJob.SalaryMin;
			var oldSalaryMax = existingJob.SalaryMax;
			var oldDepartment = existingJob.Department;
			var oldEmploymentType = existingJob.EmploymentType;
			var oldExperienceLevel = existingJob.ExperienceLevel;
			var oldBenefits = existingJob.Benefits;
			var oldExpirationDate = existingJob.ExpirationDate;

			// Map dữ liệu mới vào entity
			_mapper.Map(request, existingJob);
			existingJob.UpdatedAt = DateTime.UtcNow;

			// Update JobSkills
			if (request.SkillIds != null)
			{
				_logger.LogInformation("Updating skills for job {JobId}: {@SkillIds}",
					request.Id, request.SkillIds);
				await _uow.Jobs.UpdateJobSkillsAsync(existingJob.Id, request.SkillIds);
			}

			await _uow.Jobs.UpdateAsync(existingJob, cancellationToken);

			// Ghi audit log
			await _auditLogService.LogAsync(
				AuditEntityType.Job,
				AuditAction.UpdateJob,
				existingJob.Id.ToEntityId(),
				existingJob.Title,
				JsonSerializer.Serialize(new { oldTitle, oldLocation, oldSalaryMin, oldSalaryMax, oldDepartment, oldEmploymentType, oldExperienceLevel, oldBenefits, oldExpirationDate }),
				JsonSerializer.Serialize(new { existingJob.Title, existingJob.Location, existingJob.SalaryMin, existingJob.SalaryMax, existingJob.Department, existingJob.EmploymentType, existingJob.ExperienceLevel, existingJob.Benefits, existingJob.ExpirationDate }),
				null,
				cancellationToken);

			await _uow.SaveChangesAsync(cancellationToken);

			// Load lại job để lấy skills mới
			var updatedJob = await _uow.Jobs.GetByIdAsync(request.Id, cancellationToken);

			_logger.LogInformation("Job {JobId} updated successfully", request.Id);

			return _mapper.Map<JobDetailDto>(updatedJob);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating job {JobId}", request.Id);
			throw;
		}
	}
}