using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Role> _dbSet;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Role>();
        }

        public async Task<Role> AddAsync(Role role)
        {
            await _dbSet.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> GetById(Guid id)
        {
            var role = await _dbSet.FindAsync(id);
            return role;
        }

        public async Task<List<Role>> Where(Expression<Func<Role, bool>> predicate)
        {
            var roles = await _dbSet.Where(predicate).ToListAsync();
            return roles;
        }
    }
}
