using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Name = PermissionEnum.CreateUser.ToString(),
                        CreatedAt = DateTime.Now
                    },
                    new Permission
                    {
                        Name = PermissionEnum.CreateRole.ToString(),
                        CreatedAt = DateTime.Now
                    }
                };
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }


            if (!await context.Roles.AnyAsync())
            {
                var role = new Role
                {
                    Name = "ADMIN",
                    CreatedAt = DateTime.Now
                };
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();
            }

            if (await context.Roles.AnyAsync())
            {
                var permissions = await context.Permissions.ToArrayAsync();
                var adminRole = await context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Name.ToUpper() == "ADMIN");
                if (permissions.Any() && adminRole != null && !adminRole.Permissions.Any())
                {
                    adminRole.Permissions = permissions;
                    await context.SaveChangesAsync();
                }
            }
        
            if (!await context.Users.AnyAsync())
            {
                var user = new User
                {
                    FullName = "ADMIN",
                    Username = "ADMIN",
                    Password = "P@ssw0rd",
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
