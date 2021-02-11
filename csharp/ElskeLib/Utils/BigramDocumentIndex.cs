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
using ElskeLib.Model;

namespace ElskeLib.Utils
{
    /// <summary>
    /// A collection of integer-based documents with a bigram-based index to
    /// quickly determine the document frequency of phrases (>= 2 terms)
    /// </summary>
    public class BigramDocumentIndex
    {
        private readonly Dictionary<WordIdxBigram, List<SequencePosition>> _dictionary =
            new Dictionary<WordIdxBigram, List<SequencePosition>>();

        private readonly List<int[]> _documents = new List<int[]>();
        private readonly HashSet<int> _stopWords;

        public IReadOnlyList<int[]> Documents => _documents;

        public IReadOnlyCollection<int> StopWords => _stopWords;

        public BigramDocumentIndex()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopWords">do not add bigram keys if they only contain such stop words</param>
        public BigramDocumentIndex(HashSet<int>? stopWords)
        {
            _stopWords = stopWords;
        }

        private BigramDocumentIndex(HashSet<int> stopWords, List<int[]> documents,
            Dictionary<WordIdxBigram, List<SequencePosition>> dict) : this(stopWords)
        {
            _documents = documents;
            _dictionary = dict;
        }

        private const string StorageMetaId = "bigram-doc-index-meta.json";
        private const string StorageBlobId = "bigram-doc-index.bin";

        private class StorageMeta
        {
            public int NumBigramKeys { get; set; }
            public int NumStopWords { get; set; }
            public int NumDocuments { get; set; }
            public int Version { get; set; } = 1;
        }
        /// <summary>
        /// save index to file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix"></param>
        public void ToFile(string fn, string entryPrefix = "")
        {
            var meta = new StorageMeta
            {
                NumBigramKeys = _dictionary.Count,
                NumStopWords = _stopWords?.Count ?? -1,
                NumDocuments = _documents.Count
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
                if (_stopWords != null)
                {
                    foreach (var p in _stopWords)
                    {
                        entryStream.Write(p);
                    }
                }

                foreach (var document in _documents)
                {
                    entryStream.Write(document.Length);
                    foreach (var i in document)
                    {
                        entryStream.Write(i);
                    }
                }

                foreach (var p in _dictionary)
                {
                    entryStream.Write(p.Key.Idx1);
                    entryStream.Write(p.Key.Idx2);
                    entryStream.Write(p.Value.Count);
                    foreach (var pos in p.Value)
                    {
                        entryStream.Write(pos.DocumentIndex);
                        entryStream.Write(pos.Position);
                    }
                }
            }


        }


        /// <summary>
        /// load index from file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix"></param>
        /// <returns></returns>
        public static BigramDocumentIndex FromFile(string fn, string entryPrefix = "")
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


           

            using (var reader = new BinaryReader(new BufferedStream(zip.GetEntry(entryPrefix + StorageBlobId).Open())))
            {
                HashSet<int> stopWords = null;
                if (meta.NumStopWords > 0)
                {
                    stopWords = new HashSet<int>(meta.NumStopWords);
                    for (int i = 0; i < meta.NumStopWords; i++)
                    {
                        stopWords.Add(reader.ReadInt32());
                    }
                }

                var documents = new List<int[]>(meta.NumDocuments);
                for (int i = 0; i < meta.NumDocuments; i++)
                {
                    var len = reader.ReadInt32();
                    var arr = new int[len];
                    for (int j = 0; j < arr.Length; j++)
                    {
                        arr[j] = reader.ReadInt32();
                    }
                    documents.Add(arr);
                }

                var dict = new Dictionary<WordIdxBigram, List<SequencePosition>>(meta.NumBigramKeys);
                for (int i = 0; i < meta.NumBigramKeys; i++)
                {
                    var idx1 = reader.ReadInt32();
                    var idx2 = reader.ReadInt32();
                    var len = reader.ReadInt32();
                    var list = new List<SequencePosition>(len);
                    for (int j = 0; j < len; j++)
                    {
                        var idx = reader.ReadInt32();
                        var pos = reader.ReadInt32();
                        list.Add(new SequencePosition(idx, pos));
                    }

                    dict.Add(new WordIdxBigram(idx1, idx2), list);
                }
                
                return new BigramDocumentIndex(stopWords, documents, dict);
            }

        }



        /// <summary>
        /// Add a document to this indexed collection.
        /// </summary>
        /// <param name="doc"></param>
        public void AddDocument(int[] doc)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            var documentIndex = _documents.Count;
            _documents.Add(doc);

            for (int i = 1; i < doc.Length; i++)
            {
                var k = new WordIdxBigram(doc[i - 1], doc[i]);
                if(_stopWords != null && _stopWords.Contains(k.Idx1) && _stopWords.Contains(k.Idx2))
                    continue;
                
                _dictionary.AddToList(k, new SequencePosition(documentIndex, i-1));
            }
            
        }

        /// <summary>
        /// Get number of documents in this collection that contain the provided phrase.
        /// Will return 0 if the number of tokens is smaller than two or if the phrase only
        /// contains tokens from 'StopWords'.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int GetDocumentFrequency(ReadOnlySpan<int> tokens)
        {
            List<SequencePosition> bestList = null;
            
            var tokenOffset = 0;

            for (int i = 1; i < tokens.Length; i++)
            {
                var k = new WordIdxBigram(tokens[i - 1], tokens[i]);
                if (_dictionary.TryGetValue(k, out var list))
                {
                    if (bestList == null || bestList.Count > list.Count)
                    {
                        bestList = list;
                        tokenOffset = i - 1;
                    }
                }
                else
                {
                    if (StopWords != null && (!_stopWords.Contains(k.Idx1) || !_stopWords.Contains(k.Idx2)))
                        return 0; //we know already that there cannot be a match
                }

            }

            if (bestList == null || bestList.Count == 0)
                return 0;

            var docHashSet = new HashSet<int>();

            

            for (int i = 0; i < bestList.Count; i++)
            {
                var pos = bestList[i];
                if(docHashSet.Contains(pos.DocumentIndex))
                    continue; //we already know that this document contains the sequence

                var doc = _documents[pos.DocumentIndex];
                var startPosInDoc = pos.Position - tokenOffset;
                var endPosInDoc = startPosInDoc + tokens.Length;
                if(startPosInDoc < 0 || endPosInDoc > doc.Length)
                    continue;

                if (tokens.SequenceEqual(new ReadOnlySpan<int>(doc, startPosInDoc, tokens.Length)))
                    docHashSet.Add(pos.DocumentIndex);
            }

            return docHashSet.Count;
        }

    }

    public readonly struct SequencePosition
    {
        public SequencePosition(int documentIndex, int position)
        {
            DocumentIndex = documentIndex;
            Position = position;
        }

        public int DocumentIndex { get; }
        public int Position { get; }
    }

}
