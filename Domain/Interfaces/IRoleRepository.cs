using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> AddAsync(Role role);

        Task<Role?> GetById(Guid id);

        Task<List<Role>> WhereAsync(Expression<Func<Role, bool>> expression);
    }
}
