using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Services
{
	public class RolePermissionService : IRolePermissionService
	{
		private readonly IHostEnvironment _env;
		private readonly ILogger<RolePermissionService> _logger;
		private readonly string _configPath;
		private RoleConfig? _cachedConfig;
		private readonly SemaphoreSlim _lock = new(1, 1);
		private DateTime _lastLoadTime;
		private FileSystemWatcher? _fileWatcher;

		public RolePermissionService(IHostEnvironment env, ILogger<RolePermissionService> logger)
		{
			_env = env;
			_logger = logger;
			_configPath = Path.Combine(_env.ContentRootPath, "Configs", "role-permissions.json");
			SetupFileWatcher();

			// Load config ngay khi khởi tạo
			try
			{
				_cachedConfig = Task.Run(async () => await LoadConfigAsync()).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to load config on service initialization");
			}
		}

		private void SetupFileWatcher()
		{
			try
			{
				var directory = Path.GetDirectoryName(_configPath);
				if (directory == null || !Directory.Exists(directory)) return;

				_fileWatcher = new FileSystemWatcher(directory)
				{
					Filter = Path.GetFileName(_configPath),
					EnableRaisingEvents = true
				};

				_fileWatcher.Changed += async (sender, e) =>
				{
					_logger.LogInformation("Role permissions config file changed. Reloading...");
					await ReloadConfigAsync();
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to setup file watcher for role permissions config");
			}
		}

		private async Task<RoleConfig> LoadConfigAsync(CancellationToken cancellationToken = default)
		{
			if (_cachedConfig != null && (DateTime.UtcNow - _lastLoadTime).TotalMinutes < 5)
				return _cachedConfig;

			await _lock.WaitAsync(cancellationToken);
			try
			{
				// Double-check after acquiring lock
				if (_cachedConfig != null && (DateTime.UtcNow - _lastLoadTime).TotalMinutes < 5)
					return _cachedConfig;

				// Kiểm tra file tồn tại
				if (!File.Exists(_configPath))
				{
					_logger.LogError("Role permissions config file not found at {Path}", _configPath);
					return new RoleConfig();
				}

				// Đọc và log nội dung file để debug
				var json = await File.ReadAllTextAsync(_configPath, cancellationToken);
				_logger.LogDebug("Config file content length: {Length}", json?.Length ?? 0);

				if (string.IsNullOrWhiteSpace(json))
				{
					_logger.LogError("Config file is empty");
					return new RoleConfig();
				}

				var config = JsonSerializer.Deserialize<RoleConfig>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				_cachedConfig = config ?? new RoleConfig();
				_lastLoadTime = DateTime.UtcNow;

				_logger.LogInformation("Loaded {RoleCount} roles and {PermissionCount} permissions from config",
					_cachedConfig.Roles?.Count ?? 0,
					_cachedConfig.Permissions?.Count ?? 0);

				// Log các roles đã load để debug
				if (_cachedConfig.Roles != null)
				{
					foreach (var role in _cachedConfig.Roles)
					{
						_logger.LogDebug("Loaded role: Code={Code}, Name={Name}, Permissions={Permissions}",
							role.Code, role.Name, string.Join(",", role.Permissions));
					}
				}

				return _cachedConfig;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Invalid JSON format in config file");
				return new RoleConfig();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error loading role permissions config");
				return new RoleConfig();
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task ReloadConfigAsync(CancellationToken cancellationToken = default)
		{
			_cachedConfig = null;
			await LoadConfigAsync(cancellationToken);
		}

		public async Task<RoleConfig> GetRoleConfigAsync(CancellationToken cancellationToken = default)
		{
			return await LoadConfigAsync(cancellationToken);
		}

		public List<string> GetPermissionsForRole(string roleCode)
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Roles == null)
				{
					_logger.LogWarning("Config not loaded or empty");
					return new List<string>();
				}

				var role = config.Roles.FirstOrDefault(r =>
					r.Code != null && r.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));

				if (role == null)
				{
					_logger.LogDebug("No permissions found for role: {RoleCode}", roleCode);
					return new List<string>();
				}

				return role.Permissions ?? new List<string>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting permissions for role: {RoleCode}", roleCode);
				return new List<string>();
			}
		}

		public RoleDefinitionDto? GetRoleDefinition(string roleCode)
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Roles == null || !config.Roles.Any())
				{
					_logger.LogWarning("Config not loaded or empty");
					return null;
				}

				var role = config.Roles.FirstOrDefault(r =>
					r.Code != null && r.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));

				if (role == null)
				{
					_logger.LogDebug("No role definition found for: {RoleCode}", roleCode);

					// Log available roles for debugging
					var availableRoles = string.Join(", ", config.Roles.Select(r => r.Code));
					_logger.LogDebug("Available roles: {AvailableRoles}", availableRoles);
				}

				return role;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting role definition for: {RoleCode}", roleCode);
				return null;
			}
		}

		public List<RoleDefinitionDto> GetAllRoles()
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();
				return config?.Roles?.ToList() ?? new List<RoleDefinitionDto>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all roles");
				return new List<RoleDefinitionDto>();
			}
		}

		public bool IsValidRole(string roleCode)
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Roles == null)
					return false;

				return config.Roles.Any(r =>
					r.Code != null && r.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking valid role: {RoleCode}", roleCode);
				return false;
			}
		}

		public PermissionDefinitionDto? GetPermissionDefinition(string permissionCode)
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Permissions == null)
					return null;

				return config.Permissions.FirstOrDefault(p =>
					p.Code != null && p.Code.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting permission definition for: {PermissionCode}", permissionCode);
				return null;
			}
		}

		public List<PermissionDefinitionDto> GetAllPermissions()
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();
				return config?.Permissions?.ToList() ?? new List<PermissionDefinitionDto>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all permissions");
				return new List<PermissionDefinitionDto>();
			}
		}

		public List<PermissionDefinitionDto> GetPermissionsByGroup(string group)
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Permissions == null)
					return new List<PermissionDefinitionDto>();

				return config.Permissions
					.Where(p => p.Group != null && p.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting permissions by group: {Group}", group);
				return new List<PermissionDefinitionDto>();
			}
		}

		public bool HasPermission(string roleCode, string permissionCode)
		{
			try
			{
				var permissions = GetPermissionsForRole(roleCode);

				if (permissions.Contains("P015", StringComparer.OrdinalIgnoreCase))
					return true;

				return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking permission: {PermissionCode} for role: {RoleCode}",
					permissionCode, roleCode);
				return false;
			}
		}

		public bool HasAnyPermission(string roleCode, List<string> permissionCodes)
		{
			try
			{
				if (permissionCodes == null || !permissionCodes.Any())
					return false;

				var permissions = GetPermissionsForRole(roleCode);

				if (permissions.Contains("P015", StringComparer.OrdinalIgnoreCase))
					return true;

				return permissionCodes.Any(pc =>
					permissions.Contains(pc, StringComparer.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking any permission for role: {RoleCode}", roleCode);
				return false;
			}
		}

		public bool HasAllPermissions(string roleCode, List<string> permissionCodes)
		{
			try
			{
				if (permissionCodes == null || !permissionCodes.Any())
					return true;

				var permissions = GetPermissionsForRole(roleCode);

				if (permissions.Contains("P015", StringComparer.OrdinalIgnoreCase))
					return true;

				return permissionCodes.All(pc =>
					permissions.Contains(pc, StringComparer.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking all permissions for role: {RoleCode}", roleCode);
				return false;
			}
		}

		public UserPermissionDto GetUserPermissions(List<string> roleCodes, string language = "en")
		{
			try
			{
				var config = _cachedConfig ?? LoadConfigAsync().GetAwaiter().GetResult();

				if (config?.Roles == null || config?.Permissions == null)
				{
					_logger.LogWarning("Config not loaded or empty");
					return new UserPermissionDto();
				}

				// Get all unique permissions from roles
				var allPermissions = new HashSet<string>();
				foreach (var roleCode in roleCodes)
				{
					var role = config.Roles.FirstOrDefault(r =>
						r.Code != null && r.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));

					if (role?.Permissions != null)
					{
						foreach (var perm in role.Permissions)
						{
							allPermissions.Add(perm);
						}
					}
				}

				// Get permission definitions
				var permissionDefs = config.Permissions
					.Where(p => p.Code != null && allPermissions.Contains(p.Code))
					.ToList();

				// Group by group
				var grouped = permissionDefs
					.GroupBy(p => language == "vi" ? (p.GroupVi ?? p.Group) : (p.Group ?? p.GroupVi))
					.ToDictionary(g => g.Key ?? "Other", g => g.ToList());

				return new UserPermissionDto
				{
					Roles = roleCodes,
					Permissions = allPermissions.ToList(),
					GroupedPermissions = grouped
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user permissions");
				return new UserPermissionDto();
			}
		}

		public void Dispose()
		{
			_fileWatcher?.Dispose();
			_lock?.Dispose();
		}
	}
}