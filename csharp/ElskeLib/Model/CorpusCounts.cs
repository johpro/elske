/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ElskeLib.Model
{
    public class CorpusCounts
    {
        /// <summary>
        /// for each word (or bigram): number of documents it appears in
        /// </summary>
        public UniBigramCounts DocCounts { get; set; } = new UniBigramCounts();
        /// <summary>
        /// for each word (or bigram): number of total occurrences in corpus ('term frequency')
        /// </summary>
        public UniBigramCounts TotalCounts { get; set; } = new UniBigramCounts();


        public void ToFile(string fn, string entryPrefix = "")
        {
            DocCounts?.ToFile(fn, entryPrefix + "doc-");
            TotalCounts?.ToFile(fn,  entryPrefix + "term-");
        }

        public static CorpusCounts FromFile(string fn, string entryPrefix = "")
        {
            var res = new CorpusCounts
            {
                DocCounts = UniBigramCounts.FromFile(fn,  entryPrefix + "doc-"),
                TotalCounts = UniBigramCounts.FromFile(fn,  entryPrefix + "term-")
            };
            return res;
        }


        public static CorpusCounts GetDocCounts(IEnumerable<IEnumerable<int>> documents)
        {
            var wordCountsLocal = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>(), true);
            var pairCountsLocal = new ThreadLocal<Dictionary<WordIdxBigram, int>>(() =>
                new Dictionary<WordIdxBigram, int>(), true);
            var hashsetLocal = new ThreadLocal<HashSet<int>>(() => new HashSet<int>());
            var hashsetPairsLocal = new ThreadLocal<HashSet<WordIdxBigram>>(() =>
                new HashSet<WordIdxBigram>());
            int count = 0;

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static void ProcessDocument(IEnumerable<int> doc, 
                HashSet<int> hashset, HashSet<WordIdxBigram> hashsetPairs,
                Dictionary<int, int> wordCounts, Dictionary<WordIdxBigram, int> pairCounts)
            {
                hashset.Clear();
                hashsetPairs.Clear();
                var prevWord = -1;
                foreach (var word in doc)
                {
                    if (hashset.Add(word))
                        wordCounts.IncrementItem(word);

                    if (prevWord != -1)
                    {
                        var pair = new WordIdxBigram(prevWord, word);
                        if (hashsetPairs.Add(pair))
                            pairCounts.IncrementItem(pair);
                    }
                    prevWord = word;
                }

            }
            
            if (documents is IList<IEnumerable<int>> documentsList)
            {
                //we can use more efficient range-based multi-threading if it is a list, but at the expense of more memory
                count = documentsList.Count;
                var rangePartitioner = Partitioner.Create(0, documentsList.Count);
                Parallel.ForEach(rangePartitioner, (range, state) =>
                {
                    var wordCounts = wordCountsLocal.Value;
                    var pairCounts = pairCountsLocal.Value;
                    wordCounts.EnsureCapacity((int)(10 * Math.Sqrt(range.Item2 - range.Item1)));
                    pairCounts.EnsureCapacity(range.Item2 - range.Item1);

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var doc = documentsList[i];
                        if (doc == null)
                            continue;
                        ProcessDocument(doc, hashsetLocal.Value, hashsetPairsLocal.Value,
                            wordCounts, pairCounts);
                    }
                });
            }
            else
            {
                //do not use Parallel.ForEach as we may have IO bottleneck anyway
                //and then we can at least save memory
                var wordCounts = wordCountsLocal.Value;
                var pairCounts = pairCountsLocal.Value;
                foreach (var doc in documents)
                {
                    if (doc == null)
                        continue;
                    count++;
                    ProcessDocument(doc, hashsetLocal.Value, hashsetPairsLocal.Value,
                        wordCounts, pairCounts);
                }
            }

            var res = new CorpusCounts
            {
                DocCounts =
                {
                    WordCounts = MergeThreadLocalDictionaries(wordCountsLocal),
                    PairCounts = MergeThreadLocalDictionaries(pairCountsLocal),
                    NumDocuments = count
                }
            };
            
            wordCountsLocal.Dispose();
            pairCountsLocal.Dispose();
            hashsetLocal.Dispose();
            hashsetPairsLocal.Dispose();

            return res;

        }

        public static CorpusCounts GetDocTermCounts(IEnumerable<IEnumerable<int>> documents)
        {
            var wordCountsLocal = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>(), true);
            var hashsetLocal = new ThreadLocal<HashSet<int>>(() => new HashSet<int>());
            int count = 0;

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static void ProcessDocument(IEnumerable<int> doc,
                HashSet<int> hashset, Dictionary<int, int> wordCounts)
            {
                hashset.Clear();
                foreach (var word in doc)
                {
                    if (hashset.Add(word))
                        wordCounts.IncrementItem(word);
                }
            }

            if (documents is IList<IEnumerable<int>> documentsList)
            {
                //we can use more efficient range-based multi-threading if it is a list, but at the expense of more memory
                count = documentsList.Count;
                var rangePartitioner = Partitioner.Create(0, documentsList.Count);
                Parallel.ForEach(rangePartitioner, (range, state) =>
                {
                    var wordCounts = wordCountsLocal.Value;
                    wordCounts.EnsureCapacity((int)(10 * Math.Sqrt(range.Item2 - range.Item1)));

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var doc = documentsList[i];
                        if (doc == null)
                            continue;
                        ProcessDocument(doc, hashsetLocal.Value, wordCounts);
                    }
                });
            }
            else
            {
                var wordCounts = wordCountsLocal.Value;
                foreach (var doc in documents)
                {
                    if (doc == null)
                        continue;
                    count++;
                    ProcessDocument(doc, hashsetLocal.Value, wordCounts);
                }
            }

            var res = new CorpusCounts
            {
                DocCounts =
                {
                    WordCounts = MergeThreadLocalDictionaries(wordCountsLocal),
                    NumDocuments = count
                }
            };

            wordCountsLocal.Dispose();
            hashsetLocal.Dispose();

            return res;

        }

        public static CorpusCounts GetCounts(IEnumerable<IEnumerable<int>> documents)
        {
            var res = new CorpusCounts();
            var hashset = new HashSet<int>();
            var hashsetPairs = new HashSet<WordIdxBigram>();

            var count = 0;

            foreach (var doc in documents)
            {
                hashset.Clear();
                hashsetPairs.Clear();
                var prevWord = -1;
                foreach (var word in doc)
                {
                    res.TotalCounts.WordCounts.IncrementItem(word);
                    if (hashset.Add(word))
                        res.DocCounts.WordCounts.IncrementItem(word);

                    if (prevWord != -1)
                    {
                        var pair = new WordIdxBigram(prevWord, word);
                        res.TotalCounts.PairCounts.IncrementItem(pair);
                        if (hashsetPairs.Add(pair))
                            res.DocCounts.PairCounts.IncrementItem(pair);
                    }
                    prevWord = word;
                }

                count++;
            }
            
            res.TotalCounts.NumDocuments = count;
            res.DocCounts.NumDocuments = count;

            return res;

        }

        public static CorpusCounts GetCounts(IList<int[]> documents)
        {
            var res = new CorpusCounts();
            var hashset = new HashSet<int>();
            var hashsetPairs = new HashSet<WordIdxBigram>();

            for (int i = 0; i < documents.Count; i++)
            {
                var arr = documents[i];
                hashset.Clear();
                for (int j = 0; j < arr.Length; j++)
                {
                    var val = arr[j];
                    res.TotalCounts.WordCounts.IncrementItem(val);
                    if (hashset.Add(val))
                        res.DocCounts.WordCounts.IncrementItem(val);
                }
            }
            
            res.TotalCounts.NumDocuments = documents.Count;

            for (int i = 0; i < documents.Count; i++)
            {
                var arr = documents[i];
                hashsetPairs.Clear();
                for (int j = 1; j < arr.Length; j++)
                {
                    var val = arr[j - 1];
                    if (res.TotalCounts.WordCounts[val] <= 1)
                        continue;

                    var nextVal = arr[j];

                    if (res.TotalCounts.WordCounts[nextVal] <= 1)
                        continue;

                    var pair = new WordIdxBigram(val, nextVal);
                    res.TotalCounts.PairCounts.IncrementItem(pair);
                    if (hashsetPairs.Add(pair))
                        res.DocCounts.PairCounts.IncrementItem(pair);
                }
            }

            res.DocCounts.NumDocuments = documents.Count;
            return res;

        }





#if SINGLE_CORE_ONLY
       
        public static CorpusCounts GetTotalCountsOnly(IEnumerable<int[]> documents)
        {
            var res = new CorpusCounts();
            var counts = res.TotalCounts;

            foreach (var arr in documents)
            {
                if (arr == null || arr.Length == 0)
                    continue;

                counts.WordCounts.IncrementItem(arr[0]);

                for (int j = 1; j < arr.Length; j++)
                {
                    var val = arr[j];
                    counts.WordCounts.IncrementItem(val);
                    var pair = new WordIdxBigram(arr[j - 1], val);
                    counts.PairCounts.IncrementItem(pair);
                }
            }

            return res;
        }
#else


        public static CorpusCounts GetTotalCountsOnly(IEnumerable<int[]> documents)
        {
            var wordCountsLocal = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>(), true);
            var pairCountsLocal = new ThreadLocal<Dictionary<WordIdxBigram, int>>(() => new Dictionary<WordIdxBigram, int>(), true);
            int count = 0;
            if (documents is IList<int[]> documentsList)
            {
                //we can use more efficient range-based multi-threading if it is a list, but at the expense of more memory
                count = documentsList.Count;
                var rangePartitioner = Partitioner.Create(0, documentsList.Count);
                Parallel.ForEach(rangePartitioner, (range, state) =>
                {
                    var wordCounts = wordCountsLocal.Value;
                    var pairCounts = pairCountsLocal.Value;
                    wordCounts.EnsureCapacity((int) (10 * Math.Sqrt(range.Item2 - range.Item1)));
                    pairCounts.EnsureCapacity(range.Item2 - range.Item1);

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var arr = documentsList[i];
                        if (arr == null || arr.Length == 0)
                            continue;
                        wordCounts.IncrementItem(arr[0]);
                        for (int j = 1; j < arr.Length; j++)
                        {
                            var val = arr[j];
                            wordCounts.IncrementItem(val);
                            var pair = new WordIdxBigram(arr[j - 1], val);
                            pairCounts.IncrementItem(pair);
                        }
                    }
                });
            }
            else
            {
                var wordCounts = wordCountsLocal.Value;
                var pairCounts = pairCountsLocal.Value;
                foreach (var arr in documents)
                {
                    if (arr == null || arr.Length == 0)
                        continue;

                    count++;
                    wordCounts.IncrementItem(arr[0]);
                    for (int j = 1; j < arr.Length; j++)
                    {
                        var val = arr[j];
                        wordCounts.IncrementItem(val);
                        var pair = new WordIdxBigram(arr[j - 1], val);
                        pairCounts.IncrementItem(pair);
                    }
                }
            }

            var res = new CorpusCounts
            {
                TotalCounts =
                {
                    WordCounts = MergeThreadLocalDictionaries(wordCountsLocal),
                    PairCounts = MergeThreadLocalDictionaries(pairCountsLocal),
                    NumDocuments = count
                }
            };
            wordCountsLocal.Dispose();
            pairCountsLocal.Dispose();
            return res;
        }
#endif

        internal static Dictionary<T, int> MergeThreadLocalDictionaries<T>(ThreadLocal<Dictionary<T, int>> tLocal)
        {
            var values = tLocal.Values; //will create and return a List<T>
            if (values.Count == 0)
                return new Dictionary<T, int>();
            if (values.Count == 1)
                return values[0];
            
            if (values.Count < 4)
            {

                var r = values.MaxItem(it => it.Count);
                foreach (var d in values)
                {
                    if(d != r)
                        AddToFirstDictionary(r, d);
                }
                return r;
            }

            //we had a number of threads = dictionaries, so we should also merge dictionaries in parallel
            var numDictsPerThread = (int) Math.Ceiling(Math.Sqrt(values.Count));
            var numThreads = (int) Math.Ceiling(values.Count / (double) numDictsPerThread);

            Parallel.For(0, numThreads, t =>
            {
                var start = t * numDictsPerThread;
                var end = Math.Min(values.Count, start + numDictsPerThread);
                var dict = values[start];
                for (int i = start+1; i < end; i++)
                {
                    AddToFirstDictionary(dict, values[i]);
                }
            });
            
            //now do merge of merge
            Dictionary<T, int> res = null;
            for (int i = 0; i < values.Count; i += numDictsPerThread)
            {
                if (res == null || res.Count < values[i].Count)
                    res = values[i];
            }

            for (int i = 0; i < values.Count; i+=numDictsPerThread)
            {
                if(values[i] == res)
                    continue;
                AddToFirstDictionary(res, values[i]);
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddToFirstDictionary<T>(Dictionary<T, int> firstDict, Dictionary<T, int> secondDict)
        {
            foreach (var p in secondDict)
            {
                firstDict.AddToItem(p.Key, p.Value);
            }
            secondDict.Clear();
            secondDict.TrimExcess(0);
        }

    }
}
