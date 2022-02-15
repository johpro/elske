/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ElskeLib.Utils;

namespace ElskeLib.Model
{
    public readonly struct WordSequence : IEquatable<WordSequence>
    {
        public readonly int[] Indexes;
        public readonly int HashCode;

        public WordSequence(int[] indexes, int hashCode)
        {
            Indexes = indexes;
            HashCode = hashCode;
        }


        public WordSequence(IEnumerable<int> indexes)
        {
            Indexes = indexes.ToArray();
            HashCode = (int)Indexes.ToFnv1_32();
        }

        public WordSequence(List<int> indexes)
        {
            Indexes = indexes.ToArray();
            HashCode = (int)Indexes.ToFnv1_32();

        }

        public WordSequence(List<int> indexes, int hashCode)
        {
            Indexes = indexes.ToArray();
            HashCode = hashCode;

        }

        public static WordSequence CreateWithReference(int[] indexes)
        {
            return new WordSequence(indexes, (int)indexes.ToFnv1_32());
        }


        public bool Equals(WordSequence other)
        {
            return Indexes == other.Indexes ||
                   HashCode == other.HashCode && Indexes.Length == other.Indexes.Length &&
                   Indexes.AsSpan().SequenceEqual(other.Indexes);
        }

        public override bool Equals(object obj)
        {
            return obj is WordSequence other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public static bool operator ==(WordSequence pattern1, WordSequence pattern2)
        {
            return pattern1.Equals(pattern2);
        }

        public static bool operator !=(WordSequence pattern1, WordSequence pattern2)
        {
            return !pattern1.Equals(pattern2);
        }


        public string ToString(List<string> idxToWord)
        {
            if (Indexes == null || Indexes.Length == 0)
                return "";

            var sb = new StringBuilder();
            foreach (var idx in Indexes)
            {
                if (sb.Length > 0)
                    sb.Append(' ');

                if (idx >= 0)
                    sb.Append(idxToWord[idx]);
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(IList<int> wordList)
        {
            var idx = wordList is not int[] arr ? FindIndex(wordList, Indexes)
                : FindIndex(arr, Indexes);
            return idx >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(in WordSequence pattern)
        {
            return Equals(pattern) || Indexes.Length < pattern.Indexes.Length && Matches(pattern.Indexes);
        }


        public static int FindIndex(IList<int> listToSearch, IList<int> query)
        {
            if (listToSearch == null || listToSearch.Count == 0 || query == null || query.Count == 0)
                return -1;
            
            var a = query[0];

            if (query.Count == 1)
                return listToSearch.IndexOf(a);

            var b = query[1];
            var end = listToSearch.Count - query.Count + 1;
            for (int i = 0; i < end; i++)
            {
                if (listToSearch[i] != a || listToSearch[i + 1] != b) continue;

                var isMatch = true;
                for (int j = 2, k = i + 2; j < query.Count; j++, k++)
                {
                    if (listToSearch[k] != query[j])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                    return i;
            }

            return -1;
        }

        public static int FindIndex(int[] listToSearch, ReadOnlySpan<int> query)
        {
            if (listToSearch == null || listToSearch.Length == 0)
                return -1;

            switch (query.Length)
            {
                case 1:
                    return Array.IndexOf(listToSearch, query[0]);
                default:
                    return listToSearch.AsSpan().IndexOf(query);
            }
        }

        

        
    }
}
