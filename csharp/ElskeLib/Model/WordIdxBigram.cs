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

namespace ElskeLib.Model
{
    public readonly struct WordIdxBigram : IEquatable<WordIdxBigram>
    {
        public readonly int Idx1;
        public readonly int Idx2;

        public override bool Equals(object obj)
        {
            if (obj is WordIdxBigram p)
                return Equals(p);

            return false;
        }

        public bool Equals(WordIdxBigram other)
        {
            return Idx1 == other.Idx1 && Idx2 == other.Idx2;
        }

        public override int GetHashCode()
        {
            //this is more complex with more instructions compared to previous hash code,
            //but in the end faster because we have less collisions
            unchecked
            {
                return(int) ((((2166136261u ^ (uint)Idx1) * 16777619u) ^ (uint)Idx2) ); //return (Idx1 * 397) ^ Idx2;
            }
        }

        public static bool operator ==(WordIdxBigram pair1, WordIdxBigram pair2)
        {
            return pair1.Equals(pair2);
        }

        public static bool operator !=(WordIdxBigram pair1, WordIdxBigram pair2)
        {
            return !pair1.Equals(pair2);
        }

        public WordIdxBigram(int idxA, int idxB)
        {
            Idx1 = idxA;
            Idx2 = idxB;
        }

    }

    public class WordIdxBigramComparer : IEqualityComparer<WordIdxBigram>
    {
        public bool Equals(WordIdxBigram x, WordIdxBigram y)
        {
            return x.Idx1 == y.Idx1 && x.Idx2 == y.Idx2;
        }

        public int GetHashCode(WordIdxBigram obj)
        {
            unchecked
            {
                return (obj.Idx1 * 397) ^ obj.Idx2;
            }
        }
    }
}