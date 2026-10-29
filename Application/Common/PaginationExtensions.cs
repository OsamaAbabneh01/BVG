using Domain.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text; 

namespace Application.Common
{
    public static class PaginationExtensions
    {
        public static async Task<PaginationResultDto<T>> PagenateAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            return new PaginationResultDto<T>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
