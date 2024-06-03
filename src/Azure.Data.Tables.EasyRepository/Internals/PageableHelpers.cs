using System;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository.Internals;

internal static class PageableHelpers
{
    public static Pageable<T> CreateEnumerable<T>(Func<int?, Page<T>> firstPageFunc, Func<string?, int?, Page<T>>? nextPageFunc, int? pageSize = default) where T : notnull
    {
            var pageableHelpersType = typeof(TableClient).Assembly.GetType("Azure.Core.PageableHelpers");
            return Dynamic.InvokeMember(InvokeContext.CreateStatic(pageableHelpersType),
                new InvokeMemberName(nameof(CreateEnumerable), typeof(T)),
                firstPageFunc, nextPageFunc, pageSize);
        }
}