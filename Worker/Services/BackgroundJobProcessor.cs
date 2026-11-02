using Domain.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Workers.Models;

namespace Workers
{
    public class BackgroundJobProcessor
    {
        private readonly ILogger<BackgroundJobProcessor> _logger;
        private readonly BackgroundJobClaimer _claimer;
        private readonly BackgroundJobHeartbeat _heartbeat;
        private readonly IInvoiceRepository _invoiceRepository;

        public BackgroundJobProcessor(
            ILogger<BackgroundJobProcessor> logger,
            BackgroundJobClaimer claimer,
            BackgroundJobHeartbeat heartbeat,
            IInvoiceRepository invoiceRepository)
        {
            _logger = logger;
            _claimer = claimer;
            _heartbeat = heartbeat;
            _invoiceRepository = invoiceRepository;
        }


        public async Task ProcessAsync(
    ClaimedBackgroundJob job,
    CancellationToken cancellationToken = default)
        {
            using var processingCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            using var heartbeatCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var heartbeatTask = RunHeartbeatAsync(
                job,
                heartbeatCts,
                processingCts);

            var heartbeatStopped = false;

            async Task StopHeartbeatAsync()
            {
                if (heartbeatStopped)
                    return;

                heartbeatCts.Cancel();

                try
                {
                    await heartbeatTask;
                }
                catch (OperationCanceledException)
                {
                    // إيقاف Heartbeat بشكل طبيعي.
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Heartbeat stopped with an error. JobId: {JobId}",
                        job.Id);
                }

                heartbeatStopped = true;
            }

            try
            {
                _logger.LogInformation(
                    "Started processing JobId: {JobId}, StartInvoiceId: {StartInvoiceId}, EndInvoiceId: {EndInvoiceId}",
                    job.Id,
                    job.StartInvoiceId,
                    job.EndInvoiceId);

                // سنضيف منطق معالجة الفواتير هنا لاحقًا.
                await _invoiceRepository.ProcessInvoicesAsync(
                    job.StartInvoiceId,
                    job.EndInvoiceId,
                    processingCts.Token);

                processingCts.Token.ThrowIfCancellationRequested();

                // إيقاف Heartbeat وانتظار انتهائه أولًا.
                await StopHeartbeatAsync();

                processingCts.Token.ThrowIfCancellationRequested();

                await _claimer.CompleteAsync(
                    job.Id,
                    job.WorkerId,
                    cancellationToken);

                _logger.LogInformation(
                    "Job completed. JobId: {JobId}, StartInvoiceId: {StartInvoiceId}, EndInvoiceId: {EndInvoiceId}",
                    job.Id,
                    job.StartInvoiceId,
                    job.EndInvoiceId);
            }
            catch (OperationCanceledException)
                when (processingCts.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Job processing failed. JobId: {JobId}, StartInvoiceId: {StartInvoiceId}, EndInvoiceId: {EndInvoiceId}",
                    job.Id,
                    job.StartInvoiceId,
                    job.EndInvoiceId);

                await StopHeartbeatAsync();

                try
                {
                    await _claimer.FailAsync(
                        job.Id,
                        job.WorkerId,
                        ex.ToString(),
                        CancellationToken.None);
                }
                catch (Exception failException)
                {
                    _logger.LogError(
                        failException,
                        "Could not mark JobId: {JobId} as Failed.",
                        job.Id);
                }

                throw;
            }
            finally
            {
                await StopHeartbeatAsync();
            }
        }

        private async Task RunHeartbeatAsync(
    ClaimedBackgroundJob job,
    CancellationTokenSource heartbeatCts,
    CancellationTokenSource processingCts)
        {
            try
            {
                await _heartbeat.RunAsync(
                    job.Id,
                    job.WorkerId,
                    heartbeatCts.Token);
            }
            catch (OperationCanceledException)
                when (heartbeatCts.IsCancellationRequested)
            {
                // إيقاف Heartbeat بشكل طبيعي.
            }
            catch (Exception)
            {
                processingCts.Cancel();
                throw;
            }
        }

    }
}
