using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class BackgroundJob
{
    public long Id { get; set; }

    public Guid BatchRunId { get; set; }

    public int OfficeId { get; set; }

    public byte Status { get; set; }

    public int Priority { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? LockedUntil { get; set; }

    public string? WorkerId { get; set; }

    public string? ErrorMessage { get; set; }
}
