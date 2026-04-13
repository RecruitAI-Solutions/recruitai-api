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

	/// <summary>
	/// Tên công ty (tự động tạo nếu chưa có)
	/// </summary>
	public string? CompanyName { get; set; }

	/// <summary>
	/// Website công ty (chỉ dùng khi tạo công ty mới)
	/// </summary>
	public string? CompanyWebsite { get; set; }
}

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobDetailDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<CreateJobCommandHandler> _logger;
	private readonly IAuditLogService _auditLogService;
	private readonly IMessageService _msg;
	private readonly ISkillService _skillService;
	private readonly ICompanyService _companyService;  

	public CreateJobCommandHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<CreateJobCommandHandler> logger,
		IAuditLogService auditLogService,
		IMessageService messageService,
		ISkillService skillService,
		ICompanyService companyService)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
		_auditLogService = auditLogService;
		_msg = messageService;
		_skillService = skillService;
		_companyService = companyService;  // ⭐ GÁN
	}

	public async Task<JobDetailDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Creating job for recruiter {RecruiterId}", request.RecruiterId);

			// ========== 1. XỬ LÝ SKILL ==========
			var finalSkillIds = new List<int>();

			if (request.SkillIds != null && request.SkillIds.Any())
			{
				finalSkillIds.AddRange(request.SkillIds);
			}

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

			// ========== 2. XỬ LÝ CÔNG TY ==========
			Guid? companyId = null;

			if (!string.IsNullOrWhiteSpace(request.CompanyName))
			{
				var company = await _companyService.CreateOrGetCompanyAsync(
					request.CompanyName.Trim(),
					request.CompanyWebsite,
					request.Location,
					request.RecruiterId,
					cancellationToken);

				companyId = company.Id;
				_logger.LogInformation("Company processed: {CompanyName} (ID: {CompanyId})", company.Name, companyId);
			}

			// ========== 3. TẠO JOB ==========
			var job = _mapper.Map<Job>(request);
			job.Id = Guid.NewGuid();
			job.CreatedAt = DateTime.UtcNow;
			job.CompanyId = companyId;  // ⭐ GÁN COMPANY ID (có thể null)
			job.JobSkills = new HashSet<JobSkill>();

			// Thêm JobSkills từ finalSkillIds
			foreach (var skillId in finalSkillIds.Distinct())
			{
				job.JobSkills.Add(new JobSkill
				{
					JobId = job.Id,
					SkillId = skillId,
					IsRequired = true
				});
			}

			await _uow.Jobs.AddAsync(job, cancellationToken);

			// ========== 4. GHI AUDIT LOG ==========
			var jobData = new Dictionary<string, string>
			{
				[_msg.Get("AuditFieldTitle")] = job.Title,
				[_msg.Get("AuditFieldLocation")] = job.Location,
				["SkillCount"] = finalSkillIds.Count.ToString(),
				["SkillNames"] = string.Join(", ", request.SkillNames ?? new())
			};

			if (companyId.HasValue)
			{
				jobData["CompanyId"] = companyId.Value.ToString();
				jobData["CompanyName"] = request.CompanyName ?? "";
			}

			if (job.SalaryMin.HasValue)
				jobData[_msg.Get("AuditFieldSalaryMin")] = job.SalaryMin.Value.ToString("N0");
			if (job.SalaryMax.HasValue)
				jobData[_msg.Get("AuditFieldSalaryMax")] = job.SalaryMax.Value.ToString("N0");

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

			// Load lại job với skills và company
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
