// RecruitAI.Domain/Entities/RefreshToken.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }

        public TokenType TokenType { get; set; } = TokenType.RefreshToken;

        // Navigation
        public virtual User User { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}