using System;

namespace Azure.Data.Tables.EasyRepository;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TableAttribute" /> class.
    /// </summary>
    /// <param name="name">The name of the data table the class is mapped to.</param>
    public TableAttribute(string name)
    {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
        }

    /// <summary>
    ///     The name of the table the class is mapped to.
    /// </summary>
    public string Name { get; }
}