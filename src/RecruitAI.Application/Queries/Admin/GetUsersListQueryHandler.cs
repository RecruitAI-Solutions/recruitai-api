using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, AdminUserListResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<GetUsersListQueryHandler> _logger;

		public GetUsersListQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUsersListQueryHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<AdminUserListResponseDto> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin getting users list with filters - Page: {Page}, Role: {Role}, Status: {Status}, Keyword: {Keyword}",
				request.Page, request.Role, request.Status, request.Keyword);

			UserRole? role = null;
			if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
			{
				role = parsedRole;
			}

			UserStatus? status = request.Status.HasValue && Enum.IsDefined(typeof(UserStatus), request.Status.Value)
				? (UserStatus)request.Status.Value
				: null;

			var result = await _unitOfWork.Users.GetUsersAsync(
				request.Page,
				request.PageSize,
				role,
				status,
				request.Keyword,
				request.SortBy,
				request.SortOrder,
				cancellationToken);

			var items = result.Items.Select(user => new AdminUserSummaryDto
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Role = user.Role,
				Status = user.Status,
				EmailVerified = user.EmailVerified,
				CreatedAt = user.CreatedAt,
				LastLoginAt = user.LastLoginAt,
				PhoneNumber = user.PhoneNumber
			}).ToList();

			return new AdminUserListResponseDto
			{
				Data = items,
				Total = result.Total,
				Page = request.Page,
				PageSize = request.PageSize
			};
		}
	}
}