﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace Azure.Data.Tables.EasyRepository.Collections;

public static class BatchExtension
{
    /// <summary>
    /// Batches the source sequence into sized buckets.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
    /// <remarks>
    /// <para>
    /// This operator uses deferred execution and streams its results
    /// (buckets are streamed but their content buffered).</para>
    /// <para>
    /// When more than one bucket is streamed, all buckets except the last
    /// is guaranteed to have <paramref name="size"/> elements. The last
    /// bucket may be smaller depending on the remaining elements in the
    /// <paramref name="source"/> sequence.</para>
    /// <para>
    /// Each bucket is pre-allocated to <paramref name="size"/> elements.
    /// If <paramref name="size"/> is set to a very large value, e.g.
    /// <see cref="int.MaxValue"/> to effectively disable batching by just
    /// hoping for a single bucket, then it can lead to memory exhaustion
    /// (<see cref="OutOfMemoryException"/>).
    /// </para>
    /// </remarks>

    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
    {
            return Batch(source, size, x => x);
        }

    /// <summary>
    /// Batches the source sequence into sized buckets and applies a projection to each bucket.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
    /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <param name="resultSelector">The projection to apply to each bucket.</param>
    /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
    /// <para>
    /// This operator uses deferred execution and streams its results
    /// (buckets are streamed but their content buffered).</para>
    /// <para>
    /// <para>
    /// When more than one bucket is streamed, all buckets except the last
    /// is guaranteed to have <paramref name="size"/> elements. The last
    /// bucket may be smaller depending on the remaining elements in the
    /// <paramref name="source"/> sequence.</para>
    /// Each bucket is pre-allocated to <paramref name="size"/> elements.
    /// If <paramref name="size"/> is set to a very large value, e.g.
    /// <see cref="int.MaxValue"/> to effectively disable batching by just
    /// hoping for a single bucket, then it can lead to memory exhaustion
    /// (<see cref="OutOfMemoryException"/>).
    /// </para>

    public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
        Func<IEnumerable<TSource>, TResult> resultSelector)
    {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            switch (source)
            {
                case ICollection<TSource> { Count: 0 }:
                    {
                        return Enumerable.Empty<TResult>();
                    }
                case ICollection<TSource> collection when collection.Count <= size:
                    {
                        return _(); IEnumerable<TResult> _()
                        {
                            var bucket = new TSource[collection.Count];
                            collection.CopyTo(bucket, 0);
                            yield return resultSelector(bucket);
                        }
                    }
                case IReadOnlyCollection<TSource> { Count: 0 }:
                    {
                        return Enumerable.Empty<TResult>();
                    }
                case IReadOnlyList<TSource> list when list.Count <= size:
                    {
                        return _(); IEnumerable<TResult> _()
                        {
                            var bucket = new TSource[list.Count];
                            for (var i = 0; i < list.Count; i++)
                                bucket[i] = list[i];
                            yield return resultSelector(bucket);
                        }
                    }
                case IReadOnlyCollection<TSource> collection when collection.Count <= size:
                    {
                        return Batch(collection.Count);
                    }
                default:
                    {
                        return Batch(size);
                    }

                    IEnumerable<TResult> Batch(int size)
                    {
                        TSource[]? bucket = null;
                        var count = 0;

                        foreach (var item in source)
                        {
                            bucket ??= new TSource[size];
                            bucket[count++] = item;

                            // The bucket is fully buffered before it's yielded
                            if (count != size)
                                continue;

                            yield return resultSelector(bucket);

                            bucket = null;
                            count = 0;
                        }

                        // Return the last bucket with all remaining elements
                        if (bucket != null && count > 0)
                        {
                            Array.Resize(ref bucket, count);
                            yield return resultSelector(bucket);
                        }
                    }
            }
        }
}