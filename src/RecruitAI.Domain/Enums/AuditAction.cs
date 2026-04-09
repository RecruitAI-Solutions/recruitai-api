namespace RecruitAI.Domain.Enums
{
	public enum AuditAction
	{
		// User actions
		Create = 1,
		Update = 2,
		Delete = 3,
		Login = 4,
		Logout = 5,
		RefreshToken = 6,
		ChangePassword = 7,
		ChangeStatus = 8,
		ChangeRole = 9,

		// CV actions
		Upload = 10,
		Analyze = 11,

		// Job actions
		CreateJob = 12,
		UpdateJob = 13,
		DeleteJob = 14,

		// Application actions
		Apply = 15,
		UpdateStatus = 16
	}
}