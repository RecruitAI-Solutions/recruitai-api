using RecruitAI.Domain.Enums;

namespace RecruitAI.Shared.DTOs
{
	public class RoleConfig
	{
		public List<RoleDefinitionDto> Roles { get; set; } = new();
		public List<PermissionDefinitionDto> Permissions { get; set; } = new();
	}

	public class RoleDefinitionDto
	{
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string NameVi { get; set; } = string.Empty;
		public List<string> Permissions { get; set; } = new();
	}

	public class PermissionDefinitionDto
	{
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string NameVi { get; set; } = string.Empty;
		public string Group { get; set; } = string.Empty;
		public string GroupVi { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string DescriptionVi { get; set; } = string.Empty;
	}

	public class UserPermissionDto
	{
		public List<string> Roles { get; set; } = new();
		public List<string> Permissions { get; set; } = new();
		public Dictionary<string, List<PermissionDefinitionDto>> GroupedPermissions { get; set; } = new();
	}
}