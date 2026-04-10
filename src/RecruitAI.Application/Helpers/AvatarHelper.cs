// AvatarHelper.cs
using Microsoft.Extensions.Configuration;
using RecruitAI.Domain.Entities;

public static class AvatarHelper
{
	public static string GetAvatarUrl(string? avatarUrl, IConfiguration config)
	{
		var defaultAvatar = config["Storage:DefaultAvatarUrl"] ?? "/imgs/default_avatar/default.png";
		return string.IsNullOrEmpty(avatarUrl) ? defaultAvatar : avatarUrl;
	}
}