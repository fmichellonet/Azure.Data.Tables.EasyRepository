using System;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Data.Tables.EasyRepository.Collections;

public static class GroupByBucketExtension
{
    public static IEnumerable<IGrouping<TKey, TSource>> GroupByBucket<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, Func<TKey, int, TKey> keyIncrementer, int bucketSize)
    {
            var result = new List<IGrouping<TKey, TSource>>();
            var groups = source.GroupBy(keySelector);
            foreach (var group in groups)
            {
                var buckets = group.ToList().Batch(bucketSize).ToArray();
                
                var groupedBuckets = buckets.Select((src, idx) =>
                    new Grouping<TKey, TSource>(ComputeKey(idx, group), src));

                result.AddRange(groupedBuckets);
            }

            return result;
            
            TKey ComputeKey(int idx, IGrouping<TKey, TSource> group)
            {
                return idx == 0 ? group.Key : keyIncrementer(group.Key, idx);
            }
        }
}