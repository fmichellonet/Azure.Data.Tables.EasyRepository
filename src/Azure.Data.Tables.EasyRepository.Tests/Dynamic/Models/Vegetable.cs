using System.Collections.Generic;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;

public class Vegetable
{
    public string Name { get; set; }
    public List<string> Colors { get; set; }
}