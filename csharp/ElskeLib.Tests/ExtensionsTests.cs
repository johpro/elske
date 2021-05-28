/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElskeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ElskeLib.Utils;

namespace ElskeLib.Tests
{
    [TestClass()]
    public class ExtensionsTests
    {

        private static unsafe uint ToFnv1_32_Unsafe(ReadOnlyMemory<char> source, uint hash = Extensions.Fnv1StartHash32)
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
                        hash *= Extensions.Fnv1Prime32;
                        hash ^= p1;
                        hash *= Extensions.Fnv1Prime32;
                        len -= 4;
                        ptr = ref Unsafe.AddByteOffset(ref ptr, (IntPtr) (void*) 8);
                    }
                    while (len > 0)
                    {
                        var p = Unsafe.ReadUnaligned<ushort>(ref ptr);
                        hash ^= p;
                        hash *= Extensions.Fnv1Prime32;
                        ptr = ref Unsafe.AddByteOffset(ref ptr, (IntPtr)(void*)2);
                        len--;
                    }
                    return hash;
                }
                
            }
        }
        private static uint ToFnv1_32_Safe(ReadOnlyMemory<char> source, uint hash = Extensions.Fnv1StartHash32)
        {
            unchecked
            {
                var src = source.Span;
                for (int i = 0; i < src.Length; i++)
                {
                    hash ^= src[i];
                    hash *= Extensions.Fnv1Prime32;
                }
                return hash;
            }
        }

        [TestMethod()]
        public void ToFnv1_32_SafeUnsafeBenchmarkTest()
        {
            var extractor = KeyphraseExtractor.FromFile("../../../../../models/en-twitter.elske");
            var words = extractor.ReferenceIdxMap.ToListAsMemory();
            words.Shuffle();
            const int blockSize = 200_000;
            var i = 0;
            var sum = 0L;

            //warm-up
            var watch = Stopwatch.StartNew();
            for (int j = 0; j < blockSize; j++, i++)
            {
                sum += ToFnv1_32_Safe(words[i]);
            }
            watch.Stop();
            watch.Restart();
            for (int j = 0; j < blockSize; j++, i++)
            {
                sum += ToFnv1_32_Unsafe(words[i]);
            }
            watch.Stop();

            //run

            watch.Restart();
            for (int j = 0; j < blockSize; j++, i++)
            {
                sum += ToFnv1_32_Safe(words[i]);
            }
            watch.Stop();
            var safeTs = watch.Elapsed;
            watch.Restart();
            for (int j = 0; j < blockSize; j++, i++)
            {
                sum += ToFnv1_32_Unsafe(words[i]);
            }
            watch.Stop();
            var unsafeTs = watch.Elapsed;
            
            Trace.WriteLine($"{safeTs} safe\r\n{unsafeTs} unsafe");
        }
    }
}