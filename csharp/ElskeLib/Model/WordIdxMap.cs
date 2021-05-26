/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using ElskeLib.Utils;

namespace ElskeLib.Model
{
    public class WordIdxMap
    {
        private static readonly CharRoMemoryContentEqualityComparer MemoryComparer = new();
        /// <summary>
        /// Settings that configure how documents will be tokenized. Should not be changed
        /// after the map has been populated to avoid inconsistencies.
        /// </summary>
        public TokenizationSettings TokenizationSettings { get; set; } = new();

        private readonly Dictionary<ReadOnlyMemory<char>, int> _wordToIdx = new(MemoryComparer);
        private readonly List<ReadOnlyMemory<char>> _idxToWord = new();
        private SpinLock _spinLock = new();

        /// <summary>
        /// Number of tokens (= words) in this map.
        /// </summary>
        public int Count => _idxToWord.Count;

        private const string StorageMetaId = "word-idx-map-meta.json";
        private const string StorageBlobId = "word-idx-map.bin";

        private readonly ConcurrentBag<List<int>> _intListBag = new();
        private class StorageMeta
        {
            public TokenizationSettings TokenizationSettings { get; set; } = new();
            public int WordToIdxCount { get; set; }
            public int IdxToWordCount { get; set; }
            public int Version { get; set; } = 3;
        }

        public void ToFile(string fn, string entryPrefix = "")
        {
            var meta = new StorageMeta
            {
                IdxToWordCount = _idxToWord?.Count ?? -1,
                WordToIdxCount = _wordToIdx?.Count ?? -1,
                TokenizationSettings = TokenizationSettings
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
                //we only need to store dictionary, as we can populate list of words from it
                if (_wordToIdx != null)
                {
                    foreach (var p in _wordToIdx)
                    {
                        entryStream.Write(p.Key.ToString()); //no allocation if entire string was wrapped
                        entryStream.Write(p.Value);
                    }
                }
            }


        }

        public static WordIdxMap FromFile(string fn, string entryPrefix = "")
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

            var res = new WordIdxMap();

            using (var reader = new BinaryReader(new BufferedStream(zip.GetEntry(entryPrefix + StorageBlobId).Open())))
            {
                res.TokenizationSettings = meta.TokenizationSettings;
                if (meta.WordToIdxCount > 0)
                {
                    res._wordToIdx.EnsureCapacity(meta.WordToIdxCount);
                    res._idxToWord.Capacity = meta.IdxToWordCount;
                    //we already need to fill list so that we can later fill list by random access
                    for (int i = 0; i < meta.IdxToWordCount; i++)
                    {
                        res._idxToWord.Add(ReadOnlyMemory<char>.Empty);
                    }
                    for (int i = 0; i < meta.WordToIdxCount; i++)
                    {
                        var s = reader.ReadString();
                        var v = reader.ReadInt32();
                        var m = s.AsMemory();
                        res._wordToIdx.Add(m, v);
                        res._idxToWord[v] = m;
                    }
                }

                //uncomment block if we want to store additional data in versions > 3
                /*
                if (meta.Version <= 2 && meta.IdxToWordCount > 0)
                {
                    //for backwards compatability we still need to read block
                    for (int i = 0; i < meta.IdxToWordCount; i++)
                    {
                        reader.ReadString();
                    }
                }*/
            }

            return res;
        }

        /// <summary>
        /// Tokenizes the given document based on the parameters in TokenizationSettings
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public IEnumerable<string> TokenizeDocument(string document)
        {
            var words = document.SplitSpaces();
            if (TokenizationSettings.TwitterRemoveHashtags
                || TokenizationSettings.TwitterRemoveRetweetInfo
                || TokenizationSettings.TwitterRemoveUrls
                || TokenizationSettings.TwitterRemoveUserMentions
                || TokenizationSettings.TwitterStripHashtags)
                words = words.CleanTweets(TokenizationSettings.TwitterRemoveHashtags,
                    TokenizationSettings.TwitterRemoveUserMentions,
                    TokenizationSettings.TwitterRemoveUrls, TokenizationSettings.TwitterRemoveRetweetInfo,
                    TokenizationSettings.TwitterStripHashtags);
            words = words.Tokenize();
            if (!TokenizationSettings.RetainPunctuationCharacters)
                words = words.RemovePunctuationChars();
            if (TokenizationSettings.ConvertToLowercase)
                words = words.ToLowerInvariant();

            return words;
        }

        /// <summary>
        /// Tokenizes the given document based on the parameters in TokenizationSettings and returns an
        /// array of the corresponding integer representations of these tokens.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public int[] DocumentToIndexes(string document)
        {
            return TokensToIndexes(TokenizeDocument(document));
        }

        /// <summary>
        /// Converts a list of tokens to an array of their corresponding integer representations.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int[] TokensToIndexes(IEnumerable<string> tokens)
        {
            if (_intListBag.TryTake(out var l))
                l.Clear();
            else
                l = new List<int>();
            foreach (var w in tokens)
            {
                l.Add(GetIndex(w));
            }
            var res = l.ToArray();
            l.Clear();
            _intListBag.Add(l);
            return res;
        }

        /// <summary>
        /// Converts a list of tokens to an array of their corresponding integer representations.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int[] TokensToIndexes(IEnumerable<ReadOnlyMemory<char>> tokens)
        {
            if (_intListBag.TryTake(out var l))
                l.Clear();
            else
                l = new List<int>();
            foreach (var w in tokens)
            {
                l.Add(GetIndex(w));
            }
            var res = l.ToArray();
            l.Clear();
            _intListBag.Add(l);
            return res;
        }

        /// <summary>
        /// Converts a list of tokens to an array of their corresponding integer representations.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int[] TokensToIndexes(IList<string> tokens)
        {
            var l = new int[tokens.Count];
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                for (var i = 0; i < tokens.Count; i++)
                {
                    l[i] = GetIndexInternal(tokens[i].AsMemory());
                }
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
            return l;
        }

        /// <summary>
        /// Converts a list of tokens to an array of their corresponding integer representations.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int[] TokensToIndexes(IList<ReadOnlyMemory<char>> tokens)
        {
            var l = new int[tokens.Count];
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                for (var i = 0; i < tokens.Count; i++)
                {
                    l[i] = GetIndexInternal(tokens[i]);
                }
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
            return l;
        }


        /// <summary>
        /// Retrieves the token as ROM that the provided index represents.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<char> GetTokenAsMemory(int index)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return _idxToWord[index];
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        /// <summary>
        /// Retrieves the token as string that the provided index represents.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetToken(int index)
        {
            return GetTokenAsMemory(index).ToString();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndexInternal(ReadOnlyMemory<char> token)
        {
            if (_wordToIdx.TryGetValue(token, out var idx)) return idx;

            idx = _idxToWord.Count;
            _wordToIdx.Add(token, idx);
            _idxToWord.Add(token);

            return idx;
        }

        /// <summary>
        /// Converts a token (word) to its integer-based representation.
        /// If <paramref name="addIfNew"/> is false, it will return -1 if token was not found,
        /// otherwise it will add the token to the map.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="addIfNew">If the token is new, it will be added to the map (default).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(ReadOnlyMemory<char> token, bool addIfNew = true)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return addIfNew
                    ? GetIndexInternal(token)
                    : _wordToIdx.GetValueOrDefault(token, -1);
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        /// <summary>
        /// Converts a token (word) to its integer-based representation.
        /// If <paramref name="addIfNew"/> is false, it will return -1 if token was not found,
        /// otherwise it will add the token to the map.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="addIfNew">If the token is new, it will be added to the map (default).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(string token, bool addIfNew = true)
        {
            return GetIndex(token.AsMemory(), addIfNew);
        }

        /// <summary>
        /// Converts a list of token indexes to the corresponding phrase comprising
        /// the words the indexes represent with spaces in-between.
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public string IndexListToString(ReadOnlySpan<int> indexes)
        {
            var sb = new StringBuilder();
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                for (int i = 0; i < indexes.Length; i++)
                {
                    sb.Append(_idxToWord[indexes[i]]);
                    sb.Append(' ');
                }

                return sb.ToString(0, Math.Max(0, sb.Length - 1));
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        /// <summary>
        /// Retrieves the corresponding phrase of the provided sequence comprising
        /// the words the indexes represent with spaces in-between.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public string WordSequenceToString(WordSequence sequence)
        {
            return IndexListToString(sequence.Indexes);
        }

        internal void RemoveTokensNotPresentInDictionary<T>(IDictionary<int, T> dict)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                for (int i = _idxToWord.Count - 1; i >= 0; i--)
                {
                    if (dict.ContainsKey(i))
                        break;
                    var s = _idxToWord[i];
                    _idxToWord.RemoveAt(i);
                    _wordToIdx.Remove(s);
                }
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        

    }

    public class TokenizationSettings
    {
        /// <summary>
        /// Convert everything to lowercase
        /// </summary>
        public bool ConvertToLowercase { get; set; } = true;
        /// <summary>
        /// Keep punctuation characters such as '.', will be extra tokens
        /// </summary>
        public bool RetainPunctuationCharacters { get; set; } = false;
        /// <summary>
        /// Remove beginnings of the like 'RT @dummy: ...", relevant for tweets
        /// </summary>
        public bool TwitterRemoveRetweetInfo { get; set; }
        /// <summary>
        /// Remove #hashtag tokens
        /// </summary>
        public bool TwitterRemoveHashtags { get; set; }
        /// <summary>
        /// Strip #hashtags of '#'
        /// </summary>
        public bool TwitterStripHashtags { get; set; }
        /// <summary>
        /// Remove @user tokens
        /// </summary>
        public bool TwitterRemoveUserMentions { get; set; }
        /// <summary>
        /// Remove http://... and https://.. urls
        /// </summary>
        public bool TwitterRemoveUrls { get; set; }
    }

    internal class CharRoMemoryContentEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
        {
            return x.IsEmpty == y.IsEmpty && x.Length == y.Length && x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode(ReadOnlyMemory<char> obj)
        {
            return (int)obj.ToFnv1_32();
        }
    }
}
