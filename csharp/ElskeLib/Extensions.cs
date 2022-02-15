/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ElskeLib.Utils;

namespace ElskeLib
{
    public static class Extensions
    {
        public const uint Fnv1Prime32 = 16777619;
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
        public static unsafe uint ToFnv1_32(this ReadOnlyMemory<char> source, uint hash = Fnv1StartHash32)
        {
            unchecked
            {
                if (source.Length == 0)
                    return hash;
                var src = source.Span;
                fixed (char* srcCharPtr = src)
                {
                    ref var ptr = ref Unsafe.AsRef<byte>(srcCharPtr);
                    var len = src.Length;
                    while (len >= 4)
                    {
                        var p0 = Unsafe.ReadUnaligned<uint>(ref ptr);
                        var p1 = Unsafe.ReadUnaligned<uint>(
                            ref Unsafe.AddByteOffset(ref ptr, (IntPtr)(void*)4));
                        hash ^= p0;
                        hash *= Fnv1Prime32;
                        hash ^= p1;
                        hash *= Fnv1Prime32;
                        len -= 4;
                        ptr = ref Unsafe.AddByteOffset(ref ptr, (IntPtr)(void*)8);
                    }
                    while (len > 0)
                    {
                        var p = Unsafe.ReadUnaligned<ushort>(ref ptr);
                        hash ^= p;
                        hash *= Fnv1Prime32;
                        ptr = ref Unsafe.AddByteOffset(ref ptr, (IntPtr)(void*)2);
                        len--;
                    }
                    return hash;
                }

            }
        }
        public static unsafe uint ToFnv1_32(this List<int> source, uint hash = Fnv1StartHash32)
        {
            unchecked
            {
                fixed (int* arr = CollectionsMarshal.AsSpan(source))
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



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReferenceAt<T>(this T[] array, int i)
        {
            ref var arrayData = ref MemoryMarshal.GetArrayDataReference(array);
            ref var ri = ref Unsafe.Add(ref arrayData, i);
            return ref ri;
        }



        public static void AddToList<TKey, TVal>(this Dictionary<TKey, List<TVal>> dict, TKey key, TVal val) where TKey : notnull
        {
            ref List<TVal>? list = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            if (list == null)
            {
                list = new List<TVal>();
            }

            list.Add(val);
        }

        public static void IncrementItem<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            value++;
        }

        public static int IncrementItemAndReturn<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            value++;
            return value;
        }

        public static double AddToItem<TKey>(this Dictionary<TKey, double> dict, TKey key, double value) where TKey : notnull
        {
            ref var v = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            v += value;
            return v;
        }

        public static float AddToItem<TKey>(this Dictionary<TKey, float> dict, TKey key, float value) where TKey : notnull
        {
            ref var v = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            v += value;
            return v;
        }

        public static int AddToItem<TKey>(this Dictionary<TKey, int> dict, TKey key, int value) where TKey : notnull
        {
            ref var v = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
            v += value;
            return v;
        }




        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        public static void Shuffle<T>(this Random rng, List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
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
