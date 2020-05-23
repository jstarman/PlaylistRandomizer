using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PlaylistRandomizer
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    public static class Extensions
    {
        public static T[] Shuffle<T>(this IEnumerable<T> list)
        {
            var shuffledList = list.ToList();
            var n = list.Count();
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                var value = shuffledList[k];
                shuffledList[k] = shuffledList[n];
                shuffledList[n] = value;
            }

            return shuffledList.ToArray();
        }
    }
}