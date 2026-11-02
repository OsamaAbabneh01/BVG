using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Workers.Models;

namespace Workers
{
    public class BackgroundJobClaimer
    {
        private readonly AppDbContext _dbContext;
        private readonly BackgroundJobOptions _options;

        public BackgroundJobClaimer(
            AppDbContext dbContext,
            IOptions<BackgroundJobOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
        }

        public async Task<ClaimedBackgroundJob?> ClaimNextAsync(
            string workerId,
            CancellationToken cancellationToken = default)
        {
            var connection = _dbContext.Database.GetDbConnection();

            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync(cancellationToken);

            try
            {
                await using var command = connection.CreateCommand();

                command.CommandText = "dbo.ClaimNextBackgroundJob";
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                var workerParameter = command.CreateParameter();
                workerParameter.ParameterName = "@WorkerId";
                workerParameter.Value = workerId;
                workerParameter.DbType = DbType.String;
                command.Parameters.Add(workerParameter);

                var leaseParameter = command.CreateParameter();
                leaseParameter.ParameterName = "@LeaseSeconds";
                leaseParameter.Value = _options.LeaseSeconds;
                leaseParameter.DbType = DbType.Int32;
                command.Parameters.Add(leaseParameter);


                var maxAttemptsParameter = command.CreateParameter();
                maxAttemptsParameter.ParameterName = "@MaxAttempts";
                maxAttemptsParameter.Value = _options.MaxAttempts;
                maxAttemptsParameter.DbType = DbType.Int32;
                command.Parameters.Add(maxAttemptsParameter);


                await using var reader =
                    await command.ExecuteReaderAsync(cancellationToken);

                if (!await reader.ReadAsync(cancellationToken))
                    return null;

                return new ClaimedBackgroundJob
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    BatchRunId = reader.GetGuid(reader.GetOrdinal("BatchRunId")),
                    StartInvoiceId = reader.GetInt64(reader.GetOrdinal("StartInvoiceId")),
                    EndInvoiceId = reader.GetInt64(reader.GetOrdinal("EndInvoiceId")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    AttemptCount = reader.GetInt32(reader.GetOrdinal("AttemptCount")),
                    WorkerId = reader.GetString(reader.GetOrdinal("WorkerId")),
                    LockedUntil = reader.GetDateTime(reader.GetOrdinal("LockedUntil"))
                };
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }


        public async Task CompleteAsync(
            long jobId,
            string workerId,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.CompleteBackgroundJob @JobId = {0}, @WorkerId = {1}",
                new object[] { jobId, workerId },
                cancellationToken);
        }


        public async Task FailAsync(
    long jobId,
    string workerId,
    string errorMessage,
    CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
        EXEC dbo.FailBackgroundJob
            @JobId = {0},
            @WorkerId = {1},
            @ErrorMessage = {2},
            @MaxAttempts = {3}
        """,
                new object[]
                {
            jobId,
            workerId,
            errorMessage,
            _options.MaxAttempts
                },
                cancellationToken);
        }



        public async Task FailExpiredAsync(
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.FailExpiredBackgroundJobs @MaxAttempts = {0}",
                new object[] { _options.MaxAttempts },
                cancellationToken);
        }

        public async Task RenewAsync(
    long jobId,
    string workerId,
    CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
        EXEC dbo.RenewBackgroundJobLease
            @JobId = {0},
            @WorkerId = {1},
            @LeaseSeconds = {2}
        """,
                new object[]
                {
            jobId,
            workerId,
            _options.LeaseSeconds
                },
                cancellationToken);
        }
    }
}
