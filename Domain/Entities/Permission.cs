using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Permission
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
