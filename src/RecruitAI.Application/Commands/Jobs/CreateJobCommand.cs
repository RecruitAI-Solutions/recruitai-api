using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System.Text.Json;

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
	private readonly IAuditLogService _auditLogService;
	private readonly IMessageService _msg;

	public CreateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<CreateJobCommandHandler> logger,
		IAuditLogService auditLogService,
		IMessageService messageService)  
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
		_auditLogService = auditLogService;
		_msg = messageService;
	}

	public async Task<JobDetailDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Creating job for recruiter {RecruiterId}", request.RecruiterId);

			var job = _mapper.Map<Job>(request);
			job.Id = Guid.NewGuid();
			job.CreatedAt = DateTime.UtcNow;

			// JobSkills không bị null
			job.JobSkills = new HashSet<JobSkill>();

			// JobSkills nếu có SkillIds
			if (request.SkillIds != null && request.SkillIds.Any())
			{
				foreach (var skillId in request.SkillIds)
				{
					job.JobSkills.Add(new JobSkill
					{
						JobId = job.Id,
						SkillId = skillId,
						IsRequired = true
					});
				}
			}

			await _uow.Jobs.AddAsync(job, cancellationToken);

			// Ghi audit log
			var jobData = new Dictionary<string, string>
			{
				[_msg.Get("AuditFieldTitle")] = job.Title,
				[_msg.Get("AuditFieldLocation")] = job.Location
			};
			if (job.SalaryMin.HasValue) jobData[_msg.Get("AuditFieldSalaryMin")] = job.SalaryMin.Value.ToString("N0");
			if (job.SalaryMax.HasValue) jobData[_msg.Get("AuditFieldSalaryMax")] = job.SalaryMax.Value.ToString("N0");

			await _auditLogService.LogAsync(
				AuditEntityType.Job,
				AuditAction.CreateJob,
				job.Id.ToEntityId(),
				job.Title,
				null,
				JsonSerializer.Serialize(jobData), 
				null,
				cancellationToken);

			await _uow.SaveChangesAsync(cancellationToken);

			// Job với Include JobSkills và Skill để mapping
			var savedJob = await _uow.Jobs.GetByIdAsync(job.Id, cancellationToken);

			return _mapper.Map<JobDetailDto>(savedJob);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating job");
			throw;
		}
	}
}