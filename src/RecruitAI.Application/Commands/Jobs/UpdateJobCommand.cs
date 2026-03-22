using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

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

	public UpdateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<UpdateJobCommandHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
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