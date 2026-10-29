using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameWithPermissions(string username)
        {
            return await _context.Users
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .FirstOrDefaultAsync(u => u.Username.ToUpper().Trim() == username.Trim().ToUpper());
        }
    }
}
