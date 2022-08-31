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
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using ElskeLib.Utils;

namespace ElskeLib.Model
{
    /// <summary>
    /// Represents mapping of individual tokens (words) to their numerical representation (integer)
    /// and provides methods to tokenize documents and convert them into a sequence of corresponding integers.
    /// </summary>
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

        internal List<ReadOnlyMemory<char>> IndexToWord => _idxToWord;
        internal Dictionary<ReadOnlyMemory<char>, int> WordToIndex => _wordToIdx;

        /// <summary>
        /// Number of tokens (= words) in this map.
        /// </summary>
        public int Count => _idxToWord.Count;


        /// <summary>
        /// Create WordIdxMap instance and populate it with specified list of tokens.
        /// Identifier/index of token corresponds to its respective position in the enumeration.
        /// </summary>
        /// <param name="tokens">list of words/tokens to initialize the map with</param>
        /// <returns></returns>
        public static WordIdxMap FromTokens(IEnumerable<string> tokens)
        {
            var res = new WordIdxMap();
            if (tokens is IList<string> list)
            {
                res.IndexToWord.EnsureCapacity(list.Count);
                res.WordToIndex.EnsureCapacity(list.Count);
            }
            foreach (var token in tokens)
            {
                res.GetIndexInternal(token.AsMemory());
            }

            return res;
        }


        /// <summary>
        /// Create WordIdxMap instance and populate it with specified list of tokens.
        /// Identifier/index of token corresponds to its respective position in the enumeration.
        /// </summary>
        /// <param name="tokens">list of words/tokens to initialize the map with</param>
        /// <returns></returns>
        public static WordIdxMap FromTokens(IEnumerable<ReadOnlyMemory<char>> tokens)
        {
            var res = new WordIdxMap();
            if (tokens is IList<ReadOnlyMemory<char>> list)
            {
                res.IndexToWord.EnsureCapacity(list.Count);
                res.WordToIndex.EnsureCapacity(list.Count);
            }
            foreach (var token in tokens)
            {
                res.GetIndexInternal(token);
            }

            return res;
        }

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
                        var m = string.IsNullOrEmpty(s) ? ReadOnlyMemory<char>.Empty : s.AsMemory();
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
        public IEnumerable<ReadOnlyMemory<char>> TokenizeDocument(string document)
        {
            //we need to apply HtmlDecode and convert text to lowercase first so that we can avoid
            //allocations down the line (converting individual tokens to lowercase is expensive).
            //We still profit from the fact that we can reduce total number of objects with ReadOnlyMemory
            //instead of allocating strings for each token.
            if (TokenizationSettings.HtmlDecode)
                document = WebUtility.HtmlDecode(document);
            if (TokenizationSettings.ConvertToLowercase)
                document = document.ToLowerInvariant();

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
        /// Returns an empty span if the index is out of range.
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
                if ((uint)index >= (uint)_idxToWord.Count)
                    return ReadOnlyMemory<char>.Empty;
                //there should not be a second range check
                //as the compiler should optimize it away after inlining
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
        /// Returns an empty string if the index is out of range.
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
            ref var idx = ref CollectionsMarshal.GetValueRefOrAddDefault(_wordToIdx, token, out var exists);
            if (exists)
                return idx;
            idx = _idxToWord.Count;
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

        /// <summary>
        /// Creates and returns list of mapped tokens.
        /// The index of each item corresponds to its respective numerical representation.
        /// </summary>
        /// <returns></returns>
        public List<string> ToList()
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                var res = new List<string>(_idxToWord.Count);
                foreach (var mem in _idxToWord)
                {
                    res.Add(mem.ToString());
                }
                return res;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }
        
        /// <summary>
        /// Creates and returns list of mapped tokens as ReadOnlyMemory.
        /// The index of each item corresponds to its respective numerical representation.
        /// </summary>
        /// <returns></returns>
        public List<ReadOnlyMemory<char>> ToListAsMemory()
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return _idxToWord.ToList();
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
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
        /// Decode HTML-specific encodings such as &amp;
        /// </summary>
        public bool HtmlDecode { get; set; }
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
            return x.Length == y.Length && x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode(ReadOnlyMemory<char> obj)
        {
            return (int)obj.ToFnv1_32();
        }
    }
}
