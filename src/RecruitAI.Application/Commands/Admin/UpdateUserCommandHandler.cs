using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, AdminUserDetailResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateUserCommandHandler> _logger;
		private readonly IRolePermissionService _rolePermissionService;

		public UpdateUserCommandHandler(
			IUnitOfWork unitOfWork,
			ILogger<UpdateUserCommandHandler> logger,
			IRolePermissionService rolePermissionService)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_rolePermissionService = rolePermissionService;
		}

		public async Task<AdminUserDetailResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin updating user {UserId}", request.UserId);

			var user = await _unitOfWork.Users.GetDetailByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			// Update basic info
			if (!string.IsNullOrWhiteSpace(request.FullName))
				user.FullName = request.FullName;

			if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
				user.PhoneNumber = request.PhoneNumber;

			if (request.Gender.HasValue)
				user.Gender = request.Gender;

			if (request.DateOfBirth.HasValue)
				user.DateOfBirth = request.DateOfBirth;

			// Update status
			if (request.Status.HasValue)
			{
				user.Status = request.Status.Value;
			}

			// Update role (with permissions)
			if (request.Role.HasValue && request.Role.Value != user.Role)
			{
				var oldRole = user.Role;
				user.Role = request.Role.Value;

				var roleCode = user.Role.ToString().ToUpper();
				var permissions = _rolePermissionService.GetPermissionsForRole(roleCode);
				user.SetPermissions(permissions);

				_logger.LogInformation("User {UserId} role changed from {OldRole} to {NewRole}",
					request.UserId, oldRole, user.Role);
			}

			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

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