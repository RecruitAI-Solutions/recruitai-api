using RecruitAI.Application.DTOs.Auths;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IRolePermissionService
	{
		// Load config
		Task<RoleConfig> GetRoleConfigAsync(CancellationToken cancellationToken = default);

		// Role methods
		List<string> GetPermissionsForRole(string roleCode);
		RoleDefinitionDto? GetRoleDefinition(string roleCode);
		List<RoleDefinitionDto> GetAllRoles();
		bool IsValidRole(string roleCode);

		// Permission methods
		PermissionDefinitionDto? GetPermissionDefinition(string permissionCode);
		List<PermissionDefinitionDto> GetAllPermissions();
		List<PermissionDefinitionDto> GetPermissionsByGroup(string group);

		// Check methods
		bool HasPermission(string roleCode, string permissionCode);
		bool HasAnyPermission(string roleCode, List<string> permissionCodes);
		bool HasAllPermissions(string roleCode, List<string> permissionCodes);

		// User helper
		UserPermissionDto GetUserPermissions(List<string> roleCodes, string language = "en");

		// Reload config (khi file thay đổi)
		Task ReloadConfigAsync(CancellationToken cancellationToken = default);
	}
}