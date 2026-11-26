namespace Qwizzi.Application.Authentication;

public sealed record LoginAttemptState(bool Allowed, DateTime? LockoutUntil);
