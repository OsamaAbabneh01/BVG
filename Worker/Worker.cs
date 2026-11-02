using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Workers.Models;

namespace Workers
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;
        private readonly BackgroundJobOptions _options;

        public Worker(
            IServiceScopeFactory scopeFactory,
            ILogger<Worker> logger,
            IOptions<BackgroundJobOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var claimer = scope.ServiceProvider
                .GetRequiredService<BackgroundJobClaimer>();

            await claimer.FailExpiredAsync(stoppingToken);

            var consumers = Enumerable
                .Range(1, _options.MaxConcurrentJobs)
                .Select(id => RunConsumerAsync(id, stoppingToken));

            await Task.WhenAll(consumers);
        }

        private async Task RunConsumerAsync(
            int consumerId,
            CancellationToken stoppingToken)
        {
            var workerId =
                $"{Environment.MachineName}-Consumer-{consumerId}";

            _logger.LogInformation(
                "Consumer {ConsumerId} started. WorkerId: {WorkerId}",
                consumerId,
                workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ClaimedBackgroundJob? job;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var claimer = scope.ServiceProvider
                            .GetRequiredService<BackgroundJobClaimer>();

                        job = await claimer.ClaimNextAsync(
                            workerId,
                            stoppingToken);

                        if (job is not null)
                        {
                            var processor = scope.ServiceProvider
                                .GetRequiredService<BackgroundJobProcessor>();

                            _logger.LogInformation(
                                "Consumer {ConsumerId} claimed JobId: {JobId}",
                                consumerId,
                                job.Id);

                            await processor.ProcessAsync(
                                job,
                                stoppingToken);
                        }
                    }

                    if (job is null)
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(2),
                            stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (OperationCanceledException)
                    when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Consumer {ConsumerId} processing was cancelled.",
                        consumerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Consumer {ConsumerId} encountered an error.",
                        consumerId);

                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
            }

            _logger.LogInformation(
                "Consumer {ConsumerId} stopped.",
                consumerId);
        }
    }
}
