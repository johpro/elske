/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using ElskeLib.Utils;

namespace ElskeLib.Model
{
    public class WordSequenceRecycler
    {

        private readonly ConcurrentDictionary<int, WordSequence> _recycledPatterns = new ConcurrentDictionary<int, WordSequence>();



        public WordSequence RetrieveOrCreate(int hashCode, FastClearList<int> indexes)
        {
            if (_recycledPatterns.TryGetValue(hashCode, out var pattern))
            {
                return indexes.Storage.AsSpan(0, indexes.Count).SequenceEqual(pattern.Indexes) 
                    ? pattern : new WordSequence(indexes, hashCode);
            }

            var res = new WordSequence(indexes, hashCode);
            _recycledPatterns.TryAdd(hashCode, res);
            return res;
        }

    }
}
