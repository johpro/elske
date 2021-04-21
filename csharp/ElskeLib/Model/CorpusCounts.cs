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
        /// for each word (or bigram): number of total occurences in corpus ('term frequency')
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
                    if (hashset.Add(word))
                        res.DocCounts.WordCounts.IncrementItem(word);

                    if (prevWord != -1)
                    {
                        var pair = new WordIdxBigram(prevWord, word);
                        if (hashsetPairs.Add(pair))
                            res.DocCounts.PairCounts.IncrementItem(pair);
                    }
                    prevWord = word;
                }

                count++;
            }
            
            res.DocCounts.NumDocuments = count;

            return res;

        }

        public static CorpusCounts GetDocTermCounts(IEnumerable<IEnumerable<int>> documents)
        {
            var res = new CorpusCounts();
            var hashset = new HashSet<int>();
            var count = 0;

            foreach (var doc in documents)
            {
                hashset.Clear();
                foreach (var word in doc)
                {
                    if (hashset.Add(word))
                        res.DocCounts.WordCounts.IncrementItem(word);
                }
                count++;
            }

            res.DocCounts.NumDocuments = count;

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
            var countsLocal = new ThreadLocal<UniBigramCounts>(() => new UniBigramCounts(), true);

            int count = 0;
            if (documents is IList<int[]> documentsList)
            {
                //we can use more efficient range-based multi-threading if it is a list
                count = documentsList.Count;
                var rangePartitioner = Partitioner.Create(0, documentsList.Count);
                Parallel.ForEach(rangePartitioner, (range, state) =>
                {
                    var counts = countsLocal.Value;
                    counts.WordCounts.EnsureCapacity((int) (10 * Math.Sqrt(range.Item2 - range.Item1)));
                    counts.PairCounts.EnsureCapacity(range.Item2 - range.Item1);

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var arr = documentsList[i];

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

                });
            }
            else
            {
                Parallel.ForEach(documents, arr =>
                {
                    if (arr == null || arr.Length == 0)
                        return;

                    Interlocked.Increment(ref count);
                    var counts = countsLocal.Value;

                    counts.WordCounts.IncrementItem(arr[0]);

                    for (int j = 1; j < arr.Length; j++)
                    {
                        var val = arr[j];
                        counts.WordCounts.IncrementItem(val);
                        var pair = new WordIdxBigram(arr[j - 1], val);
                        counts.PairCounts.IncrementItem(pair);
                    }
                });
            }



            var res = new CorpusCounts();
            var values = countsLocal.Values;
            if (values.Count == 0)
                return res;


            res.TotalCounts = values[0];
            res.TotalCounts.NumDocuments = count;
            if (values.Count == 1)
                return res;

            res.TotalCounts.WordCounts.EnsureCapacity(values.Sum(v => v.WordCounts.Count));
            res.TotalCounts.PairCounts.EnsureCapacity(values.Sum(v => v.PairCounts.Count));
            for (int i = 1; i < countsLocal.Values.Count; i++)
            {
                var c = values[i];
                var counts = res.TotalCounts.WordCounts;
                foreach (var p in c.WordCounts)
                {
                    counts.AddToItem(p.Key, p.Value);
                }

                var pCounts = res.TotalCounts.PairCounts;
                foreach (var p in c.PairCounts)
                {
                    pCounts.AddToItem(p.Key, p.Value);
                }
            }

            return res;


        }
#endif
    }
}
