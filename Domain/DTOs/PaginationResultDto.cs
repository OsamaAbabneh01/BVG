using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Domain.DTOs
{
    public sealed record DynamicFilter(string Field, string Operator, JsonElement Value);

    public class DynamicFilterGroup
    {
        public string LogicalOperator { get; set; } = "AND";

        public List<DynamicFilter> Filters { get; set; } = [];

        public List<DynamicFilterGroup> Groups { get; set; } = [];
    }


    public class DynamicPaginationDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public DynamicFilterGroup Groups { get; set; } = new();
    }

    public class PaginationResultDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();

        public int TotalCount { get; set; }
    }

    public class PaginatedResultDto
    {
        public object Data { get; set; } = new object();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

    }
}
