using System.Collections.Generic;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository.Internals;

internal class QueryResponseWithHeader
{
    public IReadOnlyList<IDictionary<string, object>> Entities { get; }
    public dynamic Response { get; }
        
    public dynamic Headers { get; }

    public QueryResponseWithHeader(IReadOnlyList<IDictionary<string, object>> entities, dynamic response)
    {
            Entities = entities;
            Response = response;
            Headers = Dynamic.InvokeConstructor(typeof(TableClient).Assembly.GetType("Azure.Data.Tables.TableQueryEntitiesHeaders"),
                response);
        }
}