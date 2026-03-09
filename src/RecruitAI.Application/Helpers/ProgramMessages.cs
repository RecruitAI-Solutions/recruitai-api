using RecruitAI.Application.Resources;
using System.Resources;

namespace RecruitAI.Application.Helpers
{
    public static class ProgramMessages
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("RecruitAI.Application.Resources.Messages",
                                typeof(Messages).Assembly);

        public static string Get(string key, params object[] args)
        {
            var message = _resourceManager.GetString(key);
            if (message == null)
                return key;

            return args?.Length > 0 ? string.Format(message, args) : message;
        }

        // Helper methods cho từng loại
        public static string Log(string key, params object[] args) =>
            Get($"Log{key}", args);

        public static string Business(string key, params object[] args) =>
            Get(key, args);
    }
}