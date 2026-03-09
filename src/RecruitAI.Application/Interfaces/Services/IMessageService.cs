namespace RecruitAI.Application.Interfaces.Services
{
    public interface IMessageService
    {
        string Get(string key, params object[] args);
        string Business(string key, params object[] args);
        string Log(string key, params object[] args);
        string Validation(string key, params object[] args);
        string Success(string key, params object[] args);
    }
}