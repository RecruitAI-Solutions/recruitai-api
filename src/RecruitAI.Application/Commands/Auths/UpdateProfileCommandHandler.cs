using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Auths;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Auths
{
	public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateProfileCommandHandler> _logger;
		private readonly IValidationService _validationService;
		private readonly IMessageService _msg;
		private readonly IMediator _mediator;

		public UpdateProfileCommandHandler(
			IUnitOfWork unitOfWork,
			ILogger<UpdateProfileCommandHandler> logger,
			IValidationService validationService,
			IMessageService msg,
			IMediator mediator)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_validationService = validationService;
			_msg = msg;
			_mediator = mediator;
		}

		public async Task<UpdateProfileResponseDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("User {UserId} updating profile", request.UserId);

			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
			if (user == null)
				_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");

			// Validate phone number if provided
			if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !_validationService.IsValidPhoneNumber(request.PhoneNumber))
			{
				_msg.Throw(ErrorCode.ValidationFailed, "InvalidPhoneNumber");
			}

			// Validate date of birth if provided (cannot be in the future)
			if (request.DateOfBirth.HasValue && request.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
			{
				_msg.Throw(ErrorCode.ValidationFailed, "DateOfBirthInFuture");
			}

			var oldFullName = user.FullName;
			var oldPhoneNumber = user.PhoneNumber;
			var oldGender = user.Gender;
			var oldDateOfBirth = user.DateOfBirth;
			var oldAvatarUrl = user.AvatarUrl;

			// Update fields
			if (!string.IsNullOrWhiteSpace(request.FullName))
				user.FullName = request.FullName;

			if (request.PhoneNumber != null)
				user.PhoneNumber = request.PhoneNumber;

			if (request.Gender.HasValue)
				user.Gender = request.Gender;

			if (request.DateOfBirth.HasValue)
				user.DateOfBirth = request.DateOfBirth;

			if (request.AvatarUrl != null)
				user.AvatarUrl = request.AvatarUrl;

			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var notification = new CreateNotificationCommand
			{
				UserId = request.UserId,
				Title = _msg.Get("Notification.ProfileUpdated.Title"),
				Content = _msg.Get("Notification.ProfileUpdated.Content"),
				Type = "profile_update",
				Data = JsonSerializer.Serialize(new
				{
					UpdatedFields = new
					{
						FullName = oldFullName != user.FullName,
						PhoneNumber = oldPhoneNumber != user.PhoneNumber,
						Gender = oldGender != user.Gender,
						DateOfBirth = oldDateOfBirth != user.DateOfBirth,
						AvatarUrl = oldAvatarUrl != user.AvatarUrl
					}
				})
			};
			await _mediator.Send(notification, cancellationToken);

			_logger.LogInformation("Notification sent to user {UserId} for profile update", request.UserId);

			_logger.LogInformation("User {UserId} profile updated successfully", request.UserId);

			return new UpdateProfileResponseDto
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				PhoneNumber = user.PhoneNumber,
				Gender = user.Gender,
				DateOfBirth = user.DateOfBirth,
				AvatarUrl = user.AvatarUrl,
				UpdatedAt = user.UpdatedAt.Value
			};
		}
	}
}