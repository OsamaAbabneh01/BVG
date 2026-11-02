using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    public static class DynamicFilterExtensions
    {
        public static IQueryable<T> ApplyDynamicFilters<T>(
        this IQueryable<T> query,
        IEnumerable<DynamicFilter> filters,
        string logicalOperator = "AND")
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            var useOr = logicalOperator.Equals("OR", StringComparison.OrdinalIgnoreCase);

            if (!useOr && !logicalOperator.Equals("AND", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Operator must be AND or OR.");
            }

            foreach (var filter in filters)
            {
                var (member, guards) = GetMember(parameter, filter.Field);

                var condition = BuildCondition(member, filter);

                // Avoid null navigation errors in nested relations.
                foreach (var guard in guards)
                    condition = Expression.AndAlso(guard, condition);

                combined = combined is null
                    ? condition
                    : useOr
                        ? Expression.OrElse(combined, condition)
                        : Expression.AndAlso(combined, condition);
            }

            if (combined is null)
                return query;

            return query.Where(
                Expression.Lambda<Func<T, bool>>(combined, parameter));
        }

        private static (MemberExpression Member,List<Expression> Guards) GetMember(ParameterExpression parameter, string path)
        {
            Expression current = parameter;
            var guards = new List<Expression>();
            var parts = path.Split('.');

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    throw new ArgumentException("Invalid field path.");

                var property = current.Type.GetProperty(
                    part,
                    BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (property is null || property.GetIndexParameters().Length > 0)
                    throw new ArgumentException(
                        $"Property '{part}' does not exist in '{current.Type.Name}'.");

                current = Expression.Property(current, property);

                // Guard nullable reference navigation properties.
                if (part != parts[^1] &&
                    (!current.Type.IsValueType ||
                     Nullable.GetUnderlyingType(current.Type) is not null))
                {
                    guards.Add(Expression.NotEqual(
                        current,
                        Expression.Constant(null, current.Type)));
                }
            }

            if (current is not MemberExpression member)
                throw new ArgumentException("Invalid property path.");

            return (member, guards);
        }

        private static Expression BuildCondition(MemberExpression member, DynamicFilter filter)
        {
            var op = filter.Operator.Trim().ToLowerInvariant();
            var type = member.Type;
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (op is "isnull" or "isnotnull")
            {
                if (type.IsValueType &&
                    Nullable.GetUnderlyingType(type) is null)
                    throw new ArgumentException(
                        $"'{filter.Field}' is not nullable.");

                var isNull = Expression.Equal(
                    member, Expression.Constant(null, type));

                return op == "isnull" ? isNull : Expression.Not(isNull);
            }

            if (filter.Value.ValueKind is
                JsonValueKind.Null or JsonValueKind.Undefined)
                throw new ArgumentException(
                    $"A value is required for '{filter.Field}'.");

            if (op is "contains" or "startswith" or "endswith")
            {
                if (type != typeof(string))
                    throw new ArgumentException(
                        $"{filter.Operator} requires a string field.");

                var value = filter.Value.GetString()
                    ?? throw new ArgumentException("Invalid string value.");

                var methodName = op switch
                {
                    "contains" => nameof(string.Contains),
                    "startswith" => nameof(string.StartsWith),
                    _ => nameof(string.EndsWith)
                };

                var method = typeof(string).GetMethod(
                    methodName, [typeof(string)])!;

                var notNull = Expression.NotEqual(
                    member, Expression.Constant(null, typeof(string)));

                return Expression.AndAlso(
                    notNull,
                    Expression.Call(
                        member, method, Expression.Constant(value)));
            }

            if (op == "in")
            {
                if (filter.Value.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException("'In' requires an array.");

                var values = filter.Value.EnumerateArray()
                    .Select(v => ConvertValue(v, underlying))
                    .ToArray();

                var array = Array.CreateInstance(underlying, values.Length);

                for (var i = 0; i < values.Length; i++)
                    array.SetValue(values[i], i);

                if (values.Length == 0)
                    return Expression.Constant(false);

                Expression item = member;

                if (underlying != type)
                    item = Expression.Convert(member, underlying);

                var contains = Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Contains),
                    [underlying],
                    Expression.Constant(array, underlying.MakeArrayType()),
                    item);

                return underlying != type
                    ? Expression.AndAlso(
                        Expression.NotEqual(
                            member, Expression.Constant(null, type)),
                        contains)
                    : contains;
            }

            var valueObject = ConvertValue(filter.Value, underlying);

            Expression right = Expression.Constant(valueObject, underlying);

            if (underlying != type)
                right = Expression.Convert(right, type);

            Expression comparison = op switch
            {
                "eq" or "equals" =>
                    Expression.Equal(member, right),

                "neq" or "notequals" =>
                    Expression.NotEqual(member, right),

                "gt" or "greaterthan" =>
                    Expression.GreaterThan(member, right),

                "gte" or "greaterthanorequal" =>
                    Expression.GreaterThanOrEqual(member, right),

                "lt" or "lessthan" =>
                    Expression.LessThan(member, right),

                "lte" or "lessthanorequal" =>
                    Expression.LessThanOrEqual(member, right),

                _ => throw new ArgumentException(
                    $"Unsupported filter operator: {filter.Operator}")
            };

            if (underlying != type &&
                op is "gt" or "greaterthan" or "gte"
                    or "greaterthanorequal" or "lt" or "lessthan"
                    or "lte" or "lessthanorequal")
            {
                return Expression.AndAlso(
                    Expression.NotEqual(
                        member, Expression.Constant(null, type)),
                    comparison);
            }

            return comparison;
        }

        private static object ConvertValue(JsonElement value, Type type)
        {
            if (type == typeof(string))
                return value.GetString()
                    ?? throw new ArgumentException("Expected string.");

            if (type == typeof(Guid))
                return Guid.Parse(value.GetString()!);

            if (type == typeof(DateTime))
                return DateTime.Parse(
                    value.GetString()!,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            if (type == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(
                    value.GetString()!,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            if (type.IsEnum)
            {
                var result = value.ValueKind == JsonValueKind.String
                    ? Enum.Parse(type, value.GetString()!, true)
                    : Enum.ToObject(type, value.GetInt32());

                if (!Enum.IsDefined(type, result))
                    throw new ArgumentException("Invalid enum value.");

                return result;
            }

            if (type == typeof(bool))
                return value.GetBoolean();

            return Convert.ChangeType(
                JsonToObject(value),
                type,
                CultureInfo.InvariantCulture);
        }

        private static object JsonToObject(JsonElement value) =>
            value.ValueKind switch
            {
                JsonValueKind.String => value.GetString()!,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.GetDecimal(),
                _ => throw new ArgumentException("Unsupported value type.")
            };
    }
}
