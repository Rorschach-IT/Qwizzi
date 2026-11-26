using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Qwizzi.Api.Authentication;

public sealed class LoginChallengeTracker
{
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, AttemptState> _attempts = new(StringComparer.OrdinalIgnoreCase);
    private int _operations;

    public bool IsChallengeRequired(string email)
    {
        var key = Normalize(email);
        if (key.Length == 0 || !_attempts.TryGetValue(key, out var state))
        {
            return false;
        }

        lock (state.Gate)
        {
            if (DateTimeOffset.UtcNow - state.LastFailedAt >= AttemptWindow)
            {
                _attempts.TryRemove(key, out _);
                return false;
            }

            return state.FailedAttempts > 0;
        }
    }

    public void RecordFailure(string email)
    {
        var key = Normalize(email);
        if (key.Length == 0)
        {
            return;
        }

        var state = _attempts.GetOrAdd(key, static _ => new AttemptState());
        lock (state.Gate)
        {
            if (DateTimeOffset.UtcNow - state.LastFailedAt >= AttemptWindow)
            {
                state.FailedAttempts = 0;
            }

            state.FailedAttempts++;
            state.LastFailedAt = DateTimeOffset.UtcNow;
        }

        if (Interlocked.Increment(ref _operations) % 256 == 0)
        {
            RemoveExpiredEntries();
        }
    }

    public void Reset(string email) => _attempts.TryRemove(Normalize(email), out _);

    private void RemoveExpiredEntries()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (key, state) in _attempts)
        {
            lock (state.Gate)
            {
                if (now - state.LastFailedAt >= AttemptWindow)
                {
                    _attempts.TryRemove(key, out _);
                }
            }
        }
    }

    private static string Normalize(string email) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant())));

    private sealed class AttemptState
    {
        public object Gate { get; } = new();
        public int FailedAttempts { get; set; }
        public DateTimeOffset LastFailedAt { get; set; }
    }
}
