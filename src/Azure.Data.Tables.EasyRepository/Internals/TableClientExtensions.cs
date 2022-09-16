using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Azure.Data.Tables.EasyRepository.Serialization;
using Azure.Data.Tables.Models;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository.Internals
{
    public static class TableClientExtensions
    {
        public static Pageable<T> Query<T>(this TableClient tableClient,
            Expression<Func<TableEntityAdapter<T>, bool>>? filter = null,
            int? maxPerPage = null,
            IEnumerable<string> select = null,
            IReadOnlyCollection<IPropertySerializer<T>> serializationInformations = null,
            IReadOnlyCollection<IPropertyFlattener<T>> flatteningInformation = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            return PageableHelpers.CreateEnumerable<T>(
                pageSizeHint =>
                {
                    var queryOptions = BuildQueryOptions(tableClient, pageSizeHint, filter == null ? null : ParseFilterExpression(filter), select);

                    var response = (QueryResponseWithHeader)QueryEntities(tableClient, queryOptions, cancellationToken: cancellationToken);
                    
                    return ToPageResult<T>(response, serializationInformations, flatteningInformation);
                },
                (continuationToken, pageSizeHint) =>
                {

                    var (NextPartitionKey, NextRowKey) = ParseContinuationToken(tableClient, continuationToken);

                    var queryOptions = BuildQueryOptions(tableClient, pageSizeHint, filter == null ? null : ParseFilterExpression(filter), select);
                    
                    var response = QueryEntities(tableClient, queryOptions, NextPartitionKey, NextRowKey,
                        cancellationToken);

                    return ToPageResult<T>(response, serializationInformations, flatteningInformation);
                },
                maxPerPage);
        }
        
        private static Page<T> ToPageResult<T>(QueryResponseWithHeader response, 
            IReadOnlyCollection<IPropertySerializer<T>> serializationInformation,
            IReadOnlyCollection<IPropertyFlattener<T>> flatteningInformation) where T : class
        {
            return Page<T>.FromValues(
                TableEntityAdapter<T>.ToEntityList(response.Entities, serializationInformation, flatteningInformation),
                CreateContinuationTokenFromHeaders(response.Headers),
                response.Response);
        }

        private static QueryResponseWithHeader QueryEntities(TableClient tableClient, 
            dynamic queryOptions, string nextPartitionKey = null, string nextRowKey = null, CancellationToken cancellationToken = default)
        {
            var restClient = Dynamic.InvokeGet(tableClient, "_tableOperations");

            var response = Dynamic.InvokeMember(restClient, "QueryEntities", new object[]
            {
                new InvokeArg("table", tableClient.Name),
                new InvokeArg(nameof(queryOptions) , queryOptions),
                new InvokeArg(nameof(nextPartitionKey) , nextPartitionKey),
                new InvokeArg(nameof(nextRowKey) , nextRowKey),
                new InvokeArg(nameof(cancellationToken), cancellationToken)
            });

            return new QueryResponseWithHeader(
                Dynamic.InvokeGetChain(response, "Value.Value"),
                Dynamic.InvokeMember(response, "GetRawResponse")
            );
        }

        private static dynamic BuildQueryOptions(TableClient tableClient, int? top = 1, string? filter = null, IEnumerable<string>? projection = null)
        {
            var queryOptionsType = typeof(TableItem).Assembly.GetType("Azure.Data.Tables.Models.QueryOptions");
            var opts = Dynamic.InvokeConstructor(queryOptionsType);
            Dynamic.InvokeSet(opts, "Format", Dynamic.InvokeGetChain(tableClient, "_defaultQueryOptions.Format"));
            Dynamic.InvokeSet(opts, "Top", top);
            Dynamic.InvokeSet(opts, "Filter", filter);
            Dynamic.InvokeSet(opts, "Select", projection);
            return opts;
        }

        private static string ParseFilterExpression(Expression filter)
        {
            var strFilter = (string)Dynamic.InvokeMember(InvokeContext.CreateStatic(typeof(TableClient)),
                "Bind",
                filter);

            strFilter = strFilter.Replace("OriginalEntity/", "") ;
            return strFilter;
        }

        private static (string NextPartitionKey, string NextRowKey) ParseContinuationToken(TableClient tableClient, string continuationToken)
        {
            return Dynamic.InvokeMember(InvokeContext.CreateStatic(tableClient.GetType()),
                nameof(ParseContinuationToken), continuationToken);
        }

        private static string CreateContinuationTokenFromHeaders(dynamic headers)
        {
            return Dynamic.InvokeMember(InvokeContext.CreateStatic(typeof(TableClient)),
                nameof(CreateContinuationTokenFromHeaders), headers);
        }
    }
}