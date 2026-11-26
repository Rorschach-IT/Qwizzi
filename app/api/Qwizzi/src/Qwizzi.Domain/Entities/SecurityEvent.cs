namespace Qwizzi.Domain.Entities;

public sealed class SecurityEvent
{
    public long Sequence { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? IpAddress { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public string EntryHash { get; set; } = string.Empty;
}
