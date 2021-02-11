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
using ElskeLib.Utils;

namespace ElskeLib.Model
{
    public class WordIdxMap
    {
        public Dictionary<string, int> WordToIdx { get; private set; } = new Dictionary<string, int>();
        public List<string> IdxToWord { get; private set; } = new List<string>();
        public TokenizationSettings TokenizationSettings { get; set; } = new TokenizationSettings();

        private const string StorageMetaId = "word-idx-map-meta.json";
        private const string StorageBlobId = "word-idx-map.bin";

        private class StorageMeta
        {
            public TokenizationSettings TokenizationSettings { get; set; } = new TokenizationSettings();
            public int WordToIdxCount { get; set; }
            public int IdxToWordCount { get; set; }
            public int Version { get; set; } = 2;
        }

        public void ToFile(string fn, string entryPrefix = "")
        {
            var meta = new StorageMeta
            {
                IdxToWordCount = IdxToWord?.Count ?? -1,
                WordToIdxCount = WordToIdx?.Count ?? -1,
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
                if (WordToIdx != null)
                {
                    foreach (var p in WordToIdx)
                    {
                        entryStream.Write(p.Key);
                        entryStream.Write(p.Value);
                    }
                }

                if (IdxToWord != null)
                {
                    foreach (var s in IdxToWord)
                    {
                        entryStream.Write(s);
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

            if((meta?.Version ?? -1) < 1)
                throw new Exception("not a valid file");

            var res = new WordIdxMap();

            using (var reader = new BinaryReader(new BufferedStream(zip.GetEntry(entryPrefix + StorageBlobId).Open())))
            {
                res.TokenizationSettings = meta.TokenizationSettings;
                if (meta.WordToIdxCount > 0)
                {
                    res.WordToIdx = new Dictionary<string, int>(meta.WordToIdxCount);

                    for (int i = 0; i < meta.WordToIdxCount; i++)
                    {
                        var s = reader.ReadString();
                        var v = reader.ReadInt32();
                        res.WordToIdx.Add(s, v);
                    }
                }

                if (meta.IdxToWordCount > 0)
                {
                    res.IdxToWord.Capacity = meta.IdxToWordCount;
                    for (int i = 0; i < meta.IdxToWordCount; i++)
                    {
                        res.IdxToWord.Add(reader.ReadString());
                    }
                }
            }

            return res;
        }

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

        public List<int> DocumentToIndexes(string document)
        {
            return TokensToIndexes(TokenizeDocument(document));
        }

        public List<int> TokensToIndexes(IEnumerable<string> tokens)
        {
            var l = new List<int>();

            foreach (var w in tokens)
            {
                if (!WordToIdx.TryGetValue(w, out var idx))
                {
                    idx = IdxToWord.Count;
                    WordToIdx.Add(w, idx);
                    IdxToWord.Add(w);

                }

                l.Add(idx);
            }

            return l;
        }

        public int[] TokensToIndexes(IList<string> tokens)
        {
            var l = new int[tokens.Count];
            for (var i = 0; i < tokens.Count; i++)
            {
                var w = tokens[i];
                if (!WordToIdx.TryGetValue(w, out var idx))
                {
                    idx = IdxToWord.Count;
                    WordToIdx.Add(w, idx);
                    IdxToWord.Add(w);
                }

                l[i] = idx;
            }

            return l;
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
}
