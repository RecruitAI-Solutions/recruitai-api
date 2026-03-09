using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests
{
    public class RegisterRequestDto
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }
    }
}
