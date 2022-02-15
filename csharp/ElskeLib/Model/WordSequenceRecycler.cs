/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ElskeLib.Model
{
    public class WordSequenceRecycler
    {
        private readonly ConcurrentDictionary<int, WordSequence> _recycledPatterns = new();
        
        public WordSequence RetrieveOrCreate(int hashCode, List<int> indexes)
        {
            if (_recycledPatterns.TryGetValue(hashCode, out var pattern))
            {
                return CollectionsMarshal.AsSpan(indexes).SequenceEqual(pattern.Indexes) 
                    ? pattern : new WordSequence(indexes, hashCode);
            }

            var res = new WordSequence(indexes, hashCode);
            _recycledPatterns.TryAdd(hashCode, res);
            return res;
        }

    }
}
