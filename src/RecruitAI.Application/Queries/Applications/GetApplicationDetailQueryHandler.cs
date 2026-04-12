using MediatR;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;
using RecruitAI.Application.Helpers;

namespace RecruitAI.Application.Queries.Applications
{
	public class GetApplicationDetailQueryHandler : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMessageService _msg;

		public GetApplicationDetailQueryHandler(IUnitOfWork unitOfWork, IMessageService msg)
		{
			_unitOfWork = unitOfWork;
			_msg = msg;
		}

		public async Task<ApplicationDetailResponseDto> Handle(GetApplicationDetailQuery request, CancellationToken cancellationToken)
		{
			var application = await _unitOfWork.JobApplications.GetDetailByIdAsync(request.ApplicationId, cancellationToken);

			if (application == null)
				_msg.Throw(ErrorCode.ResourceNotFound, "ApplicationNotFound");

			// Check permission
			var isCandidate = application.CV?.UserId == request.UserId;
			var isRecruiter = application.Job?.RecruiterId == request.UserId;

			if (!isCandidate && !isRecruiter)
				_msg.Throw(ErrorCode.Forbidden, "NoPermissionToViewApplication");

			// Build response
			var response = new ApplicationDetailResponseDto
			{
				Id = application.Id,
            Status = application.Status,
			StatusName = application.Status.ToString(),
			StatusDisplay = _msg.Get($"ApplicationStatus.{application.Status}"),
				AppliedAt = application.AppliedAt,
				ReviewedAt = application.ReviewedAt,
				Notes = application.Notes,
				Job = new JobInfoDto
				{
					Id = application.Job.Id,
					Title = application.Job.Title,
					Description = application.Job.Description,
					Requirements = application.Job.Requirements,
					Location = application.Job.Location,
					SalaryMin = application.Job.SalaryMin,
					SalaryMax = application.Job.SalaryMax,
					Currency = application.Job.Currency,
					EmploymentType = application.Job.EmploymentType,
					ExperienceLevel = application.Job.ExperienceLevel,
					Department = application.Job.Department,
					Benefits = application.Job.Benefits,
					ExpirationDate = application.Job.ExpirationDate
				},
				Cv = new CvInfoDto
				{
					Id = application.CV.Id,
					FileName = application.CV.FileName,
					FileSize = application.CV.FileSize,
					UploadedAt = application.CV.UploadedAt,
					DownloadUrl = $"/api/v1/cv/{application.CV.Id}/download"
				},
				Candidate = new CandidateInfoDto
				{
					Id = application.CV.User.Id,
					FullName = application.CV.User.FullName,
					Email = application.CV.User.Email,
					PhoneNumber = application.CV.User.PhoneNumber,
					AvatarUrl = application.CV.User.AvatarUrl
				}
			};

			// Add match result if exists
			if (application.Match != null)
			{
				var matchedSkills = JsonSerializer.Deserialize<List<SkillDetailDto>>(application.Match.MatchedSkillsJson) ?? new();
				var missingSkills = JsonSerializer.Deserialize<List<SkillDetailDto>>(application.Match.MissingSkillsJson) ?? new();

				response.MatchResult = new MatchResultDto
				{
					MatchPercentage = application.Match.MatchPercentage,
					MatchedSkillCount = application.Match.MatchedSkillCount,
					RequiredSkillCount = application.Match.RequiredSkillCount,
					MatchedSkills = matchedSkills,
					MissingSkills = missingSkills,
					CalculatedAt = application.Match.CalculatedAt
				};
			}

			return response;
		}
	}
}