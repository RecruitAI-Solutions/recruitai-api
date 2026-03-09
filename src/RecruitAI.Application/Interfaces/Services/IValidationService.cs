namespace RecruitAI.Application.Interfaces.Services
{
    public interface IValidationService
    {
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellation = default);
    }

}
