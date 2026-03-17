using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TicketService.Infrastructure.Observability;

public sealed class ObservabilityLogWriter
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string? _logPath;

    public ObservabilityLogWriter(IConfiguration configuration)
    {
        _logPath = configuration["Observability:LogPath"]?.Trim();
        if (string.IsNullOrWhiteSpace(_logPath))
        {
            _logPath = null;
            return;
        }

        var directory = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task WriteAsync(object payload, CancellationToken cancellationToken = default)
    {
        if (_logPath is null)
        {
            return;
        }

        try
        {
            var line = JsonSerializer.Serialize(payload);

            await _lock.WaitAsync(cancellationToken);
            try
            {
                await File.AppendAllTextAsync(_logPath, line + Environment.NewLine, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }
        catch
        {
            // Observability writes must never break the main request flow.
        }
    }
}
