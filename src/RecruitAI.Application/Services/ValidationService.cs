using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Services
{
	public class ValidationService : IValidationService
	{
		private readonly IUnitOfWork _uow;

		public ValidationService(IUnitOfWork uow)
		{
			_uow = uow;
		}

		/// <summary>
		/// Kiểm tra email đã tồn tại trong hệ thống chưa
		/// </summary>
		public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			var user = await _uow.Users.GetByEmailAsync(email, cancellationToken);
			return user == null;
		}

		/// <summary>
		/// Kiểm tra user có tồn tại không
		/// </summary>
		public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			if (userId == Guid.Empty)
				return false;

			return await _uow.Users.AnyAsync(u => u.Id == userId, cancellationToken);
		}

		/// <summary>
		/// Kiểm tra email có thuộc về user không (dùng khi update)
		/// </summary>
		public async Task<bool> IsEmailBelongToUserAsync(string email, Guid userId, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(email) || userId == Guid.Empty)
				return false;

			var user = await _uow.Users.GetByEmailAsync(email, cancellationToken);

			if (user == null)
				return true;

			return user.Id == userId;
		}

		/// <summary>
		/// Kiểm tra role có hợp lệ không
		/// </summary>
		public bool IsValidRole(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
				return false;

			return Enum.TryParse<Domain.Enums.UserRole>(role, true, out _);
		}

		/// <summary>
		/// Kiểm tra mật khẩu có đủ mạnh không
		/// </summary>
		public bool IsStrongPassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
				return false;

			// Có ít nhất 1 chữ hoa, 1 chữ thường, 1 số
			bool hasUpper = password.Any(char.IsUpper);
			bool hasLower = password.Any(char.IsLower);
			bool hasDigit = password.Any(char.IsDigit);

			return hasUpper && hasLower && hasDigit;
		}

		/// <summary>
		/// Kiểm tra định dạng email
		/// </summary>
		public bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Kiểm tra số điện thoại (Việt Nam)
		/// </summary>
		public bool IsValidPhoneNumber(string phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
				return false;

			// Regex đơn giản cho số điện thoại VN (10 số, bắt đầu bằng 0)
			return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^(0|\+84)[3-9][0-9]{8}$");
		}
	}
}