/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace ElskeLib.Model
{

    public class UniBigramCounts
    {
        public Dictionary<WordIdxBigram, int> PairCounts { get; set; } = new Dictionary<WordIdxBigram, int>();
        public Dictionary<int, int> WordCounts { get; set; } = new Dictionary<int, int>();

        public int NumDocuments { get; set; }


        private const string StorageMetaId = "uni-bigram-counts-meta.json";
        private const string StorageBlobId = "uni-bigram-counts.bin";

        private class StorageMeta
        {
            public int PairCountsCount { get; set; }
            public int WordCountsCount { get; set; }
            public int NumDocuments { get; set; }
            public int Version { get; set; } = 1;
        }
        /// <summary>
        /// save counts to file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix"></param>
        public void ToFile(string fn, string entryPrefix = "")
        {
            var meta = new StorageMeta
            {
                PairCountsCount = PairCounts?.Count ?? -1,
                WordCountsCount = WordCounts?.Count ?? -1,
                NumDocuments = NumDocuments
            };
            var metaStr = JsonSerializer.Serialize(meta);
            using var stream = new FileStream(fn, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Update);
            zip.GetEntry(entryPrefix + StorageMetaId)?.Delete();
            var entry = zip.CreateEntry(entryPrefix + StorageMetaId);
            using (var entryStream = new StreamWriter(entry.Open()))
                entryStream.Write(metaStr);

            zip.GetEntry(entryPrefix + StorageBlobId)?.Delete();
            entry = zip.CreateEntry(entryPrefix + StorageBlobId);
            using (var entryStream = new BinaryWriter(new BufferedStream(entry.Open())))
            {
                if (PairCounts != null)
                {
                    foreach (var p in PairCounts)
                    {
                        entryStream.Write(p.Key.Idx1);
                        entryStream.Write(p.Key.Idx2);
                        entryStream.Write(p.Value);
                    }
                }

                if (WordCounts != null)
                {
                    foreach (var p in WordCounts)
                    {
                        entryStream.Write(p.Key);
                        entryStream.Write(p.Value);
                    }
                }
            }


        }


        /// <summary>
        /// load counts from file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix"></param>
        /// <returns></returns>
        public static UniBigramCounts FromFile(string fn, string entryPrefix = "")
        {
            using var stream = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = zip.GetEntry(entryPrefix + StorageMetaId);
            if (entry == null)
                return null;

            StorageMeta meta = null;
            using (var reader = new StreamReader(entry.Open()))
                meta = JsonSerializer.Deserialize<StorageMeta>(reader.ReadToEnd());

            if ((meta?.Version ?? -1) < 1)
                throw new Exception("not a valid file");

            var res = new UniBigramCounts{NumDocuments = meta.NumDocuments};

            using (var reader = new BinaryReader(new BufferedStream(zip.GetEntry(entryPrefix + StorageBlobId).Open())))
            {

                if (meta.PairCountsCount > 0)
                {
                    res.PairCounts.EnsureCapacity(meta.PairCountsCount);

                    for (int i = 0; i < meta.PairCountsCount; i++)
                    {
                        var k1 = reader.ReadInt32();
                        var k2 = reader.ReadInt32();
                        var v = reader.ReadInt32();
                        res.PairCounts.Add(new WordIdxBigram(k1, k2), v);
                    }
                }

                if (meta.WordCountsCount > 0)
                {
                    res.WordCounts.EnsureCapacity(meta.WordCountsCount);
                    for (int i = 0; i < meta.WordCountsCount; i++)
                    {
                        var k = reader.ReadInt32();
                        var v = reader.ReadInt32();
                        res.WordCounts.Add(k, v);
                    }
                }
            }

            return res;
        }

        public double GetIdf(int wordIdx)
        {
            return Math.Log(NumDocuments / (double) Math.Max(1, WordCounts.GetValueOrDefault(wordIdx)));
        }


        /// <summary>
        /// save space by removing entries in WordCounts and PairCounts that occur rarely, e.g., only once
        /// </summary>
        /// <param name="minCount"></param>
        public void RemoveEntriesBelowThreshold(int minCount = 2)
        {
            if (minCount <= 1)
                return;

            if (WordCounts != null)
            {
                var numTermsBelowTh = 0;
                foreach (var count in WordCounts.Values)
                {
                    if (count < minCount)
                        numTermsBelowTh++;
                }

                if (numTermsBelowTh > 0)
                {
                    var capacity = WordCounts.Count - numTermsBelowTh;
                    var newDict = new Dictionary<int, int>(capacity);
                    if (capacity > 0)
                    {
                        foreach (var p in WordCounts)
                        {
                            if(p.Value >= minCount)
                                newDict.Add(p.Key, p.Value);
                        }
                    }

                    WordCounts = newDict;
                }
            }

            if (PairCounts != null)
            {
                var numTermsBelowTh = 0;
                foreach (var count in PairCounts.Values)
                {
                    if (count < minCount)
                        numTermsBelowTh++;
                }

                if (numTermsBelowTh > 0)
                {
                    var capacity = PairCounts.Count - numTermsBelowTh;
                    var newDict = new Dictionary<WordIdxBigram, int>(capacity);
                    if (capacity > 0)
                    {
                        foreach (var p in PairCounts)
                        {
                            if (p.Value >= minCount)
                                newDict.Add(p.Key, p.Value);
                        }
                    }

                    PairCounts = newDict;
                }
            }
        }
    }
}