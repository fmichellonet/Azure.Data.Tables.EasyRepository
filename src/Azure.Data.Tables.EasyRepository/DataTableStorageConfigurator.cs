using System;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Data.Tables.EasyRepository;

public static class DataTableStorageConfigurator
{
    public static IServiceCollection AddDataTables(this IServiceCollection services,
        string connectionString, Action<DataTableConfiguration> configure)
    {
        var serviceClient = new TableServiceClient(connectionString);
        services.AddTransient(x => serviceClient);
        configure(new DataTableConfiguration(services));
        return services;
    }
}
