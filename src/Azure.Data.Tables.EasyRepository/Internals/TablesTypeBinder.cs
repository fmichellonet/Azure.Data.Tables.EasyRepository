using Dynamitey;
using System.Collections.Generic;

namespace Azure.Data.Tables.EasyRepository.Internals;

internal class TablesTypeBinder
{
    private readonly dynamic _surrogateType;

    private static TablesTypeBinder? _instance;

    private TablesTypeBinder(dynamic surrogateType)
    {
        _surrogateType = surrogateType;
    }

    public static TablesTypeBinder Shared()
    {
        if (_instance != null)
        {
            return _instance;
        }
        var tablesTypeBinderTypeName = "Azure.Data.Tables.TablesTypeBinder";
        var tablesTypeBinderType = typeof(TableClient).Assembly.GetType(tablesTypeBinderTypeName);
        var propertyBackingAccessorName = "get_Shared";

        var surrogate = Dynamic.InvokeMember(InvokeContext.CreateStatic(tablesTypeBinderType), propertyBackingAccessorName);
        _instance = new TablesTypeBinder(surrogate);
        return _instance;
    }

    public BoundTypeInfo<T> GetBinderInfo<T>()
    {
        var invocation = new CacheableInvocation(InvocationKind.InvokeMember,
            new InvokeMemberName(nameof(GetBinderInfo)),
            argCount: 1, context: _surrogateType.GetType());

        var typeInfo = invocation.Invoke(_surrogateType, typeof(T));
        return new BoundTypeInfo<T>(typeInfo);
    }
}

internal class BoundTypeInfo<T>
{
    private readonly dynamic _surrogateType;

    public BoundTypeInfo(dynamic surrogateType)
    {
        _surrogateType = surrogateType;
    }

    public void Serialize(T entity, Dictionary<string, object> dictionary)
    {
        var invocation = new CacheableInvocation(InvocationKind.InvokeMemberAction,
            new InvokeMemberName(nameof(Serialize), typeof(T)),
            argCount: 2, context: _surrogateType.GetType());

        invocation.Invoke(_surrogateType, new object[] { entity, dictionary });
    }

    public T Deserialize(IDictionary<string, object> dictionary)
    {
        var invocation = new CacheableInvocation(InvocationKind.InvokeMember,
            new InvokeMemberName(nameof(Deserialize), typeof(T)),
            argCount: 1, context: _surrogateType.GetType());

        return invocation.Invoke(_surrogateType, new object[] { dictionary });
    }
}