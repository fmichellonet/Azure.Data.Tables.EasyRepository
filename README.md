# Azure.Data.Tables.EasyRepository

[![NuGet Badge](https://buildstats.info/nuget/AzureDataTables.EasyRepository)](https://www.nuget.org/packages/AzureDataTables.EasyRepository/)

## About
The `Azure.Data.Tables.EasyRepository` library is an extension designed to simplify the use of Azure Table Storage service in your .NET projects. 
It provides an abstraction layer that eases CRUD (Create, Read, Update, Delete) operations and entity management.

## Features
- **CRUD Operations Abstraction**: Simplifies entity management by providing methods inspired by LINQ for creating, reading, updating, and deleting entities : 
  <code>AddAsync()</code>, <code>SingleAsync()</code>, <code>ToListAsync()</code>, <code>WhereAsync()</code>, <code>MergeRangeAsync()</code>, <code>DeleteRangeAsync()</code>, and much more.
- **No ITableEntity interface leakeage**: Works with models that do not implement <code>ITableEntity</code>.
- **Complex types support**: you can now use complex class hierarchies. You have the flexibility to choose whether to serialize (defaulting to JSON) or flatten the properties of your complex object. 
This feature enables you to work with more intricate data structures while maintaining efficiency and ease of use.
- **Custom serializer support**:When dealing with scenarios that require the utmost finesse, you can even define your own custom serializer. This approach allows you to tailor serialization to your specific needs, ensuring that your data is handled precisely as desired
- **Dependency injection ready & fluent configuration**: Designed to seamlessly integrate with Microsoft.Extensions.DependencyInjection and offers a fluent API that simplifies the configuration of your repositories. With this approach, you can effortlessly set up and manage your data access components

## Getting Started
To use `Azure.Data.Tables.EasyRepository` :

1. Add a reference to the nuget package in your project.

```bash
dotnet add package AzureDataTables.EasyRepository
```

2. Create a model

```csharp
public class Employee
{
    public string FirstName { get; set; }
        
    public string LastName { get; set; }

    public string Email { get; set; }

    public double? AnnualSalary { get; set; }

    public DateTime CreationDate { get; set; }

    public string CreatedBy { get; set; }
}
```
*note that this class do not implement __ITableEntity__*

3. Configure the repository

```csharp
services.AddDataTables(connectionString, conf =>
{
    conf.AddDynamicRepositoryFor<Employee>(x => x.CreationDate.Value.ToString("yyyy"), x => x.Email);
});
```

4. Use the repository

```csharp
public class EmployeeHandler
{
    private readonly IDynamicTableRepository<Employee> _repository;

    public EnumerateConsultantHandler(IDynamicTableRepository<Employee> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Employee>> HandleAsync(CancellationToken cancellationToken)
    {
        return await _repository.WhereAsync(x => x.OriginalEntity.Email.Contains("Smith"),
                cancellationToken: cancellationToken);
    }
}
```