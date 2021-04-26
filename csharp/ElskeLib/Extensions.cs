using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ElskeLib.Utils;

namespace ElskeLib
{
    public static class Extensions
    {
        const uint Fnv1Prime32 = 16777619;
        public const uint Fnv1StartHash32 = 2166136261;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToFnv1_32(this uint source, uint hash = Fnv1StartHash32)
        {
            unchecked
            {
                hash ^= source;
                hash *= Fnv1Prime32;
            }
            return hash;
        }

        public static uint ToFnv1_32(this IList<int> source, uint hash = Fnv1StartHash32)
        {
            unchecked
            {
                for (int i = 0; i < source.Count; i++)
                {
                    hash ^= (uint)source[i];
                    hash *= Fnv1Prime32;
                }
                return hash;
            }
        }
        public static unsafe uint ToFnv1_32(this FastClearList<int> source, uint hash = Fnv1StartHash32)
        {
            unchecked
            {
                fixed (int* arr = source.Storage)
                {
                    var count = source.Count;
                    for (int i = 0; i < count; i++)
                    {
                        hash ^= (uint)arr[i];
                        hash *= Fnv1Prime32;
                    }
                }
                return hash;
            }
        }




        public static void AddToList<TKey, TVal>(this Dictionary<TKey, List<TVal>> dict, TKey key, TVal val)
        {
            if (!dict.TryGetValue(key, out var l))
            {
                l = new List<TVal>();
                dict.Add(key, l);
            }

            l.Add(val);
        }

        public static void IncrementItem<TKey>(this Dictionary<TKey, int> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var val))
            {
                dict[key] = val + 1;
            }
            else
            {
                dict.Add(key, 1);
            }
        }

        public static int IncrementItemAndReturn<TKey>(this Dictionary<TKey, int> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var val))
            {
                val++;
                dict[key] = val;
                return val;
            }


            dict.Add(key, 1);
            return 1;

        }

        public static double AddToItem<TKey>(this Dictionary<TKey, double> dict, TKey key, double value)
        {
            if (dict.TryGetValue(key, out var val))
            {
                var newVal = val + value;
                dict[key] = newVal;
                return newVal;
            }

            dict.Add(key, value);
            return value;
        }

        public static int AddToItem<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
        {
            if (dict.TryGetValue(key, out var val))
            {
                var newVal = val + value;
                dict[key] = newVal;
                return newVal;
            }

            dict.Add(key, value);
            return value;
        }


        public static TVal MaxItem<TVal>(this IEnumerable<TVal> arr, Func<TVal, DateTime> expression)
        {
            var maxV = DateTime.MinValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl > maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }

        public static TVal MaxItem<TVal>(this ReadOnlySpan<TVal> arr, Func<TVal, DateTime> expression)
        {
            var maxV = DateTime.MinValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl > maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }

        public static TVal MaxItem<TVal>(this IEnumerable<TVal> arr, Func<TVal, int> expression)
        {
            var maxV = int.MinValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl > maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }

        public static TVal MaxItem<TVal>(this IEnumerable<TVal> arr, Func<TVal, double> expression)
        {
            var maxV = double.MinValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl > maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }

        public static TVal MinItem<TVal>(this IEnumerable<TVal> arr, Func<TVal, int> expression)
        {
            var maxV = int.MaxValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl < maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }

        public static TVal MinItem<TVal>(this IEnumerable<TVal> arr, Func<TVal, double> expression)
        {
            var maxV = double.MaxValue;
            var ret = default(TVal);
            foreach (var val in arr)
            {
                var vl = expression(val);
                if (vl < maxV)
                {
                    maxV = vl;
                    ret = val;
                }
            }

            return ret;
        }



        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this Random rng, List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this T[] array)
        {
            new Random(Guid.NewGuid().GetHashCode()).Shuffle(array);
        }
        public static void Shuffle<T>(this List<T> list)
        {
            new Random(Guid.NewGuid().GetHashCode()).Shuffle(list);
        }



        public static int PickRandom(this int[] array, Random rnd, int forbiddenIndex = -1)
        {
            int rn;
            do
            {
                rn = rnd.Next(array.Length);
            } while (rn == forbiddenIndex);

            return array[rn];

        }


    }
}
