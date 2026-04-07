using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, AdminUserDetailResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<GetUserDetailQueryHandler> _logger;

		public GetUserDetailQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserDetailQueryHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<AdminUserDetailResponseDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin getting user detail for UserId: {UserId}", request.UserId);

			var user = await _unitOfWork.Users.GetDetailByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			return new AdminUserDetailResponseDto
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Role = user.Role,
				Status = user.Status,
				EmailVerified = user.EmailVerified,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				LastLoginAt = user.LastLoginAt,
				Gender = user.Gender,
				DateOfBirth = user.DateOfBirth,
				PhoneNumber = user.PhoneNumber,
				AvatarUrl = user.AvatarUrl,
				Permissions = user.GetPermissionList()
			};
		}
	}
}