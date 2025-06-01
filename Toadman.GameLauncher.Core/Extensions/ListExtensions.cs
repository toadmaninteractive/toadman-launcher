using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toadman.GameLauncher.Core
{
    public static class ListExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int degreeOfParallelism, Func<T, Task> body, IProgress<T> progress = null)
        {
            return Task.WhenAll(
                Partitioner.Create(source).GetPartitions(degreeOfParallelism)
                    .Select(partition => Task.Run(async () => {
                        using (partition)
                            while (partition.MoveNext())
                            {
                                await body(partition.Current);
                                progress?.Report(partition.Current);
                            }
                    }))
            );
        }
    }
}