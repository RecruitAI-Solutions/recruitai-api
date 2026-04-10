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

	// Kỹ năng - HỖ TRỢ CẢ 2 CÁCH
	public List<int> SkillIds { get; set; } = new();           // Cách 1: Dùng ID có sẵn
	public List<string> SkillNames { get; set; } = new();      // Cách 2: Dùng tên (tự động tạo mới)

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
	private readonly ISkillService _skillService;  

	public CreateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<CreateJobCommandHandler> logger,
		IAuditLogService auditLogService,
		IMessageService messageService,
		ISkillService skillService)  
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
		_auditLogService = auditLogService;
		_msg = messageService;
		_skillService = skillService;
	}

	public async Task<JobDetailDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Creating job for recruiter {RecruiterId}", request.RecruiterId);

			// XỬ LÝ SKILL: TỰ ĐỘNG TẠO SKILL MỚI TỪ TÊN
			var finalSkillIds = new List<int>();

			// 1. Thêm các skill từ SkillIds (nếu có)
			if (request.SkillIds != null && request.SkillIds.Any())
			{
				finalSkillIds.AddRange(request.SkillIds);
			}

			// 2. Xử lý các skill từ SkillNames (tự động tạo mới nếu chưa có)
			if (request.SkillNames != null && request.SkillNames.Any())
			{
				foreach (var skillName in request.SkillNames)
				{
					if (string.IsNullOrWhiteSpace(skillName)) continue;

					var trimmedName = skillName.Trim();
					var skill = await _skillService.CreateOrGetSkillAsync(trimmedName);
					finalSkillIds.Add(skill.Id);

					_logger.LogInformation("Auto-created/found skill: {SkillName} (ID: {SkillId})", trimmedName, skill.Id);
				}
			}

			var job = _mapper.Map<Job>(request);
			job.Id = Guid.NewGuid();
			job.CreatedAt = DateTime.UtcNow;
			job.JobSkills = new HashSet<JobSkill>();

			// Thêm JobSkills từ finalSkillIds
			foreach (var skillId in finalSkillIds.Distinct())  // Distinct để tránh trùng
			{
				job.JobSkills.Add(new JobSkill
				{
					JobId = job.Id,
					SkillId = skillId,
					IsRequired = true
				});
			}

			await _uow.Jobs.AddAsync(job, cancellationToken);

			// Ghi audit log
			var jobData = new Dictionary<string, string>
			{
				[_msg.Get("AuditFieldTitle")] = job.Title,
				[_msg.Get("AuditFieldLocation")] = job.Location,
				["SkillCount"] = finalSkillIds.Count.ToString(),
				["SkillNames"] = string.Join(", ", request.SkillNames ?? new())
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

			// Load lại job với skills
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