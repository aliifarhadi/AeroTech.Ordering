using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Consumers.Outbox
{
    public sealed class OutboxPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxPublisherOptions _options;
        private readonly ILogger<OutboxPublisher> _logger;
        private readonly string _leaseOwner = $"{Environment.MachineName}:{Guid.NewGuid():N}";

        public OutboxPublisher(
            IServiceScopeFactory scopeFactory,
            IOptions<OutboxPublisherOptions> options,
            ILogger<OutboxPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Max(1, _options.PublishIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

                    await dispatcher.DispatchPendingAsync(_leaseOwner, stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Outbox publishing loop failed.");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
