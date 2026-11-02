using Domain.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);

        Task<User?> GetById(Guid id);

        Task<List<User>> WhereAsync(Expression<Func<User, bool>> expression);

        //Task<List<User>> GetAllAsync();

        Task<User?> GetByUsernameWithPermissions(string username);

        IQueryable<User> Query(List<DynamicFilter> filters);
    }
}
