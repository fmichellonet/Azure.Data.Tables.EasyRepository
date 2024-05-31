using System.Collections.Generic;
using System.Linq;
using Azure.Data.Tables.EasyRepository.Collections;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Collections;

public class GroupByBucketShould
{
    [Test]
    public void ComputeCorrectBuckets()
    {
            var all = Enumerable.Range(1, 100).Select(x => x.ToString()).ToList();

            IEnumerable<IGrouping<string, string>> res = all.GroupByBucket(x => x.Length.ToString(), (src, increment) => $"+{increment}_{src}", 10);

            var dictionary = res.ToDictionary(x => x.Key, x => x.ToList());

            Assert.That(dictionary["1"], Has.Count.EqualTo(9)); // 1-9
            Assert.That(dictionary["2"], Has.Count.EqualTo(10)); // 10-19
            Assert.That(dictionary.ContainsKey("+1_2"), Is.True); // 20-29
            Assert.That(dictionary.ContainsKey("+2_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+3_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+4_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+5_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+6_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+7_2"), Is.True);
            Assert.That(dictionary.ContainsKey("+8_2"), Is.True); // 90-99
            Assert.That(dictionary.ContainsKey("3"), Is.True); // 1
        }
}