namespace Qwizzi.Domain.Entities;

public sealed class SecurityAuditState
{
    public int Id { get; set; }
    public long LastSequence { get; set; }
    public string LastHash { get; set; } = string.Empty;
}
