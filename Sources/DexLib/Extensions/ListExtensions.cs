using System;
using System.Collections.Generic;

namespace Dot42.DexLib.Extensions
{
    public static class ListExtensions
    {
        private static readonly Random rnd = new Random();

        public static void Shuffle<T>(this List<T> list)
        {
            if (list.Count > 1)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    T item = list[i];
                    int index = rnd.Next(i + 1);

                    list[i] = list[index];
                    list[index] = item;
                }
            }
        }
    }
}