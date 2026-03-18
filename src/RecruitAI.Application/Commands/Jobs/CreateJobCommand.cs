using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Jobs;

public class CreateJobCommand : IRequest<JobDetailDto>
{
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

	// Sẽ được gán từ claims
	public Guid RecruiterId { get; set; }
}

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobDetailDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<CreateJobCommandHandler> _logger;

	public CreateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<CreateJobCommandHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<JobDetailDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Creating job for recruiter {RecruiterId}", request.RecruiterId);

			var job = _mapper.Map<Job>(request);
			job.Id = Guid.NewGuid();
			job.CreatedAt = DateTime.UtcNow;

			// Lưu job trước
			await _uow.Jobs.AddAsync(job, cancellationToken);

			// JobSkills
			if (request.SkillIds.Any())
			{
				await _uow.Jobs.AddJobSkillsAsync(job.Id, request.SkillIds);
			}

			_logger.LogInformation("Job created successfully with ID: {JobId}", job.Id);

			return _mapper.Map<JobDetailDto>(job);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating job");
			throw;
		}
	}
}
