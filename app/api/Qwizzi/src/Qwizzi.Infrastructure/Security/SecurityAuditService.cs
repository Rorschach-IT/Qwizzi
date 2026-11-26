using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Qwizzi.Application.Security;
using Qwizzi.Domain.Entities;
using Qwizzi.Infrastructure.Persistence;

namespace Qwizzi.Infrastructure.Security;

public sealed class SecurityAuditService(AppDbContext dbContext) : ISecurityAuditService
{
    private const long AdvisoryLockKey = 723490183;

    public async Task RecordAsync(Guid? userId, string? username, string? ipAddress, string action, string description, bool succeeded, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({AdvisoryLockKey})",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT set_config('qwizzi.audit_write', 'on', true)",
            cancellationToken);

        var state = await dbContext.SecurityAuditStates
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (state is null)
        {
            if (await dbContext.SecurityEvents.AnyAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "The security audit checkpoint is missing while audit events exist. Manual integrity recovery is required.");
            }

            state = new SecurityAuditState { Id = 1 };
            dbContext.SecurityAuditStates.Add(state);
        }
        var entry = new SecurityEvent
        {
            Sequence = state.LastSequence + 1,
            UserId = userId,
            Username = Truncate(username, 50),
            IpAddress = Truncate(ipAddress, 64),
            OccurredAt = DateTime.UtcNow,
            Action = Truncate(action, 100) ?? string.Empty,
            Description = Truncate(description, 2000) ?? string.Empty,
            Succeeded = succeeded,
            PreviousHash = state.LastHash
        };
        entry.EntryHash = ComputeHash(entry);
        dbContext.SecurityEvents.Add(entry);
        state.LastSequence = entry.Sequence;
        state.LastHash = entry.EntryHash;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<AuditVerificationResult> VerifyAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({AdvisoryLockKey})",
            cancellationToken);
        var entries = await dbContext.SecurityEvents
            .AsNoTracking()
            .OrderBy(entry => entry.Sequence)
            .ToListAsync(cancellationToken);
        var state = await dbContext.SecurityAuditStates
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (state is null)
        {
            var invalid = new AuditVerificationResult(false, 1, entries.Count);
            await transaction.CommitAsync(cancellationToken);
            return invalid;
        }

        var previousHash = string.Empty;
        long expectedSequence = 1;
        foreach (var entry in entries)
        {
            if (entry.Sequence != expectedSequence
                || !FixedEquals(entry.PreviousHash, previousHash)
                || !FixedEquals(entry.EntryHash, ComputeHash(entry)))
            {
                var invalid = new AuditVerificationResult(false, entry.Sequence, expectedSequence - 1);
                await transaction.CommitAsync(cancellationToken);
                return invalid;
            }

            previousHash = entry.EntryHash;
            expectedSequence++;
        }

        if (state.LastSequence != entries.Count)
        {
            var invalid = new AuditVerificationResult(false, Math.Min(state.LastSequence, entries.Count) + 1, entries.Count);
            await transaction.CommitAsync(cancellationToken);
            return invalid;
        }
        if (!FixedEquals(state.LastHash, previousHash))
        {
            var invalid = new AuditVerificationResult(false, state.LastSequence == 0 ? 1 : state.LastSequence, entries.Count);
            await transaction.CommitAsync(cancellationToken);
            return invalid;
        }

        var valid = new AuditVerificationResult(true, null, entries.Count);
        await transaction.CommitAsync(cancellationToken);
        return valid;
    }

    public async Task<IReadOnlyList<SecurityEventDto>> GetRecentAsync(int limit, CancellationToken cancellationToken = default)
    {
        var entries = await dbContext.SecurityEvents
            .AsNoTracking()
            .OrderByDescending(entry => entry.Sequence)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return entries
            .OrderBy(entry => entry.Sequence)
            .Select(entry => new SecurityEventDto(entry.Sequence, entry.UserId, entry.Username, entry.OccurredAt, entry.IpAddress, entry.Action, entry.Description, entry.Succeeded))
            .ToList();
    }

    private static string ComputeHash(SecurityEvent entry)
    {
        var value = string.Join('\n',
            entry.Sequence.ToString(System.Globalization.CultureInfo.InvariantCulture),
            entry.UserId?.ToString("D") ?? string.Empty,
            entry.Username ?? string.Empty,
            entry.OccurredAt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            entry.IpAddress ?? string.Empty,
            entry.Action,
            entry.Description,
            entry.Succeeded ? "1" : "0",
            entry.PreviousHash);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static bool FixedEquals(string left, string right)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(left),
            Encoding.ASCII.GetBytes(right));
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is { Length: > 0 } ? value[..Math.Min(value.Length, maxLength)] : value;
}
