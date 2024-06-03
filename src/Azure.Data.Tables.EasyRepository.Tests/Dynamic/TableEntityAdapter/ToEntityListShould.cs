using System;
using System.Collections.Generic;
using System.Diagnostics;
using Azure.Data.Tables.EasyRepository.Serialization;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.TableEntityAdapter;

public class ToEntityListShould
{
    [Test]
    public void Perform_under_1_second_per_100000_items()
    {
        var sw = new Stopwatch();
        sw.Start();

        var list = new List<Dictionary<string, object>>();
        for (var i = 0; i < 100000; i++)
        {
            var source = new Dictionary<string, object>()
            {
                { "Name", $"Person_{i}" },
                { "Age", i }
            };
            list.Add(source);
        }

        var res = TableEntityAdapter<MyModel>.ToEntityList(list, Array.Empty<IPropertySerializer<MyModel>>(),
            Array.Empty<IPropertyFlattener<MyModel>>());

        sw.Stop();

        Assert.That(sw.Elapsed.TotalSeconds, Is.LessThan(1));
    }
}

public class MyModel
{
    public string Name { get; set; }
    public int Age { get; set; }
}