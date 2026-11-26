namespace Qwizzi.Application.Security;

public interface ISecurityAuditService
{
    Task RecordAsync(Guid? userId, string? username, string? ipAddress, string action, string description, bool succeeded, CancellationToken cancellationToken = default);
    Task<AuditVerificationResult> VerifyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SecurityEventDto>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}

public sealed record AuditVerificationResult(bool IsIntact, long? FirstInvalidSequence, long VerifiedEntries);
public sealed record SecurityEventDto(long Sequence, Guid? UserId, string? Username, DateTime OccurredAt, string? IpAddress, string Action, string Description, bool Succeeded);
