using Microsoft.Extensions.Hosting;

namespace SilverKinetics.w80.Application.Services;

public abstract class IntervalBackgroundServiceBase
    : IHostedService, IDisposable
{

    public IntervalBackgroundServiceBase(int runInternvalInSeconds) {
        _runInternalInSeconds = runInternvalInSeconds;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runInternalInSeconds > 10)
        {
            _timer = new Timer(
                callback: async (e) => { await ExecuteAsync(cancellationToken).ConfigureAwait(false); },
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromSeconds(_runInternalInSeconds));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private Timer? _timer = null;
    private readonly int _runInternalInSeconds = 0;
}
