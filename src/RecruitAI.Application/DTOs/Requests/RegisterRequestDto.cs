using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests
{
    public class RegisterRequestDto
    {
        public UserRole Role { get; set; } = UserRole.Candidate;
        public string Email { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }
        public Gender? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
