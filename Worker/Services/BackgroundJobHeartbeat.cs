using System;
using System.Collections.Generic;
using System.Text;
using Workers;

namespace Workers
{
    public class BackgroundJobHeartbeat
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackgroundJobHeartbeat> _logger;

        public BackgroundJobHeartbeat(
            IServiceScopeFactory scopeFactory,
            ILogger<BackgroundJobHeartbeat> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RunAsync(
            long jobId,
            string workerId,
            CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(60));

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                using var scope = _scopeFactory.CreateScope();

                var claimer = scope.ServiceProvider
                    .GetRequiredService<BackgroundJobClaimer>();

                await claimer.RenewAsync(
                    jobId,
                    workerId,
                    cancellationToken);

                _logger.LogInformation(
                    "Lease renewed. JobId: {JobId}, WorkerId: {WorkerId}",
                    jobId,
                    workerId);
            }
        }
    }
}
