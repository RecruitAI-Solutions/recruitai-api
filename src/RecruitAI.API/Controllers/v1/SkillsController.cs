using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests.Skill;
using RecruitAI.Application.DTOs.Responses.Skill;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI_API.Controllers.v1;
using System.Security.Claims;

namespace RecruitAI.API.Controllers.v1
{
	[ApiController]
	[Route("api/v1/skills")]
	[Authorize]
	public class SkillsController : BaseController
	{
		private readonly IUnitOfWork _uow;

		public SkillsController(
			IMediator mediator,
			ILogger<SkillsController> logger,
			IMessageService messageService,
			IUnitOfWork uow,
			IWorkContext workContext)
			: base(mediator, logger, messageService, workContext)
		{
			_uow = uow;
		}

		/// <summary>
		/// Lấy danh sách skills với phân trang và lọc
		/// </summary>
		[HttpGet]
		[ProducesResponseType(typeof(SkillSearchResponseDto), StatusCodes.Status200OK)]
		public async Task<ActionResult<SkillSearchResponseDto>> Search(
			[FromQuery] SkillSearchRequestDto request,
			CancellationToken cancellationToken)
		{
			return await ExecuteAsync<SkillSearchResponseDto>(async () =>
			{
				var (items, totalCount) = await _uow.Skills.SearchAsync(
					keyword: request.Keyword,
					category: request.Category,
					isActive: request.IsActive,
					page: request.Page,
					pageSize: request.PageSize,
					sortBy: request.SortBy,
					sortOrder: request.SortOrder,
					cancellationToken: cancellationToken
				);

				var response = new SkillSearchResponseDto
				{
					Items = items.Select(MapToResponseDto).ToList(),
					TotalCount = totalCount,
					Page = request.Page,
					PageSize = request.PageSize
				};

				return response;
			});
		}

		/// <summary>
		/// Lấy chi tiết skill theo ID
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(SkillResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<SkillResponseDto>> GetById(
			int id,
			CancellationToken cancellationToken)
		{
			return await ExecuteAsync<SkillResponseDto>(async () =>
			{
				var skill = await _uow.Skills.GetByIdAsync(id, cancellationToken);

				if (skill == null)
					throw new BusinessException(ErrorCode.SkillNotFound, _msg.Business("SkillNotFound"));

				return MapToResponseDto(skill);
			});
		}

		/// <summary>
		/// Lấy danh sách categories
		/// </summary>
		[HttpGet("categories")]
		[ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<string>>> GetCategories(CancellationToken cancellationToken)
		{
			return await ExecuteAsync<IEnumerable<string>>(async () =>
			{
				return await _uow.Skills.GetAllCategoriesAsync(cancellationToken);
			});
		}

		/// <summary>
		/// Gợi ý skills cho autocomplete
		/// </summary>
		[HttpGet("suggest")]
		[ProducesResponseType(typeof(IEnumerable<SkillSuggestionDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<SkillSuggestionDto>>> Suggest(
			[FromQuery] string q,
			[FromQuery] int limit = 10,
			CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<IEnumerable<SkillSuggestionDto>>(async () =>
			{
				if (string.IsNullOrWhiteSpace(q) || limit <= 0)
					return new List<SkillSuggestionDto>();

				var suggestions = await _uow.Skills.SuggestAsync(q, Math.Min(limit, 50), cancellationToken);

				return suggestions.Select(s => new SkillSuggestionDto
				{
					Id = s.Id,
					Name = s.Name,
					Category = s.Category
				});
			});
		}

		/// <summary>
		/// Tạo skill mới (Admin only)
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(SkillResponseDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<SkillResponseDto>> Create(
			[FromBody] CreateSkillRequestDto request,
			CancellationToken cancellationToken)
		{
			return await ExecuteAsync<SkillResponseDto>(async () =>
			{
				// Kiểm tra trùng tên
				if (await _uow.Skills.ExistsByNameAsync(request.Name, cancellationToken))
				{
					throw new BusinessException(ErrorCode.SkillAlreadyExists,
						_msg.Business("SkillAlreadyExists", request.Name));
				}

				var skill = new Skill
				{
					Name = request.Name,
					Category = request.Category,
					Aliases = request.Aliases,
					ContextKeywords = request.ContextKeywords,
					CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "system"
				};

				await _uow.Skills.AddAsync(skill, cancellationToken);
				await _uow.SaveChangesAsync(cancellationToken);

				return MapToResponseDto(skill);
			}, "SkillCreated");
		}

		/// <summary>
		/// Cập nhật skill (Admin only)
		/// </summary>
		[HttpPut("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(SkillResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<SkillResponseDto>> Update(
			int id,
			[FromBody] UpdateSkillRequestDto request,
			CancellationToken cancellationToken)
		{
			return await ExecuteAsync<SkillResponseDto>(async () =>
			{
				var existing = await _uow.Skills.GetByIdAsync(id, cancellationToken);

				if (existing == null)
					throw new BusinessException(ErrorCode.SkillNotFound, _msg.Business("SkillNotFound"));

				// Kiểm tra trùng tên (nếu tên thay đổi)
				if (existing.Name != request.Name &&
					await _uow.Skills.ExistsByNameAsync(request.Name, cancellationToken))
				{
					throw new BusinessException(ErrorCode.SkillAlreadyExists,
						_msg.Business("SkillAlreadyExists", request.Name));
				}

				existing.Name = request.Name;
				existing.Category = request.Category;
				existing.Aliases = request.Aliases;
				existing.ContextKeywords = request.ContextKeywords;
				existing.IsActive = request.IsActive;
				existing.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;

				await _uow.Skills.UpdateAsync(existing, cancellationToken);
				await _uow.SaveChangesAsync(cancellationToken);

				return MapToResponseDto(existing);
			}, "SkillUpdated");
		}

		/// <summary>
		/// Xóa skill (soft delete) - Admin only
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
		{
			return await ExecuteAsync(async () =>
			{
				if (!await _uow.Skills.ExistsAsync(id, cancellationToken))
					throw new BusinessException(ErrorCode.SkillNotFound, _msg.Business("SkillNotFound"));

				await _uow.Skills.DeleteAsync(id, cancellationToken);
				await _uow.SaveChangesAsync(cancellationToken);
			}, "SkillDeleted");
		}

		private static SkillResponseDto MapToResponseDto(Skill skill)
		{
			return new SkillResponseDto
			{
				Id = skill.Id,
				Name = skill.Name,
				Category = skill.Category,
				Aliases = skill.Aliases,
				ContextKeywords = skill.ContextKeywords,
				IsActive = skill.IsActive,
				CreatedAt = skill.CreatedAt,
				UpdatedAt = skill.UpdatedAt,
				CreatedBy = skill.CreatedBy
			};
		}
	}
}