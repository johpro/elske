/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace ElskeLib.Utils
{
    public static class Tokenizer
    {
        public static readonly char[] SpaceDelimiters =
        {
            ' ', ' ' /*this is the no-break space, &nbsp;*/, '\r', '\n', '\t', '\v', '\u008a', '	',
            '\u2000','\u2001','\u2002','\u2003','\u2004','\u2005','\u2006','\u2007','\u2008','\u2009',
            '\u200a', '\u202f', '\u205f', '\u3000'
        };

        public static readonly HashSet<char> SpaceDelimitersSet = new(SpaceDelimiters);


        public static readonly char[] PunctuationChars =
        {
            '.', '…', ',', '"', ';', '!', '?', '&', '(', ')', ':', '„', '“', '<', '>', '|', '｜', '«', '»',
            '‛', '‟', '‹', '›', '‘', '’', '’', '\u201A', '_', '^', '\u2022' /*bullet point*/,
            '/', '\\', '[', ']', '“', '”', '{', '}', '—', '–', '-', '\'', '*', '=' //* important for wiki and *bold* text, = for wiki header markup
        };


        /// <summary>
        /// Characters that may be part of a word (e.g, didn't), but are also punctuation chars or quotation marks
        /// </summary>
        public static readonly char[] SpecialPunctuationChars = { '-', '’', '\'' };

        public static readonly HashSet<char> PunctuationCharsSet = new(PunctuationChars);


        public static readonly HashSet<int> EmojisUtf32Set = new(EmojiHelper.ListOfEmojisUtf32);

        private static readonly ConcurrentBag<FastClearList<(int startIdx, int length)>> SlicingListBag = new();


        public static IEnumerable<ReadOnlyMemory<char>> SplitSpaces(this string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                yield break;
            }

            var startIdx = -1;
            for (int i = 0; i < src.Length; i++)
            {
                var chr = src[i];
                if (SpaceDelimitersSet.Contains(chr))
                {
                    if (startIdx >= 0)
                    {
                        yield return src.AsMemory(startIdx, i - startIdx);
                    }
                    startIdx = -1;
                    continue;
                }
                if (startIdx == -1)
                {
                    startIdx = i;
                }
            }
            if (startIdx == 0)
            {
                yield return src.AsMemory();
            }
            if (startIdx > 0)
            {
                yield return src.AsMemory(startIdx, src.Length - startIdx);
            }
        }


        public static IEnumerable<ReadOnlyMemory<char>> CleanTweets(this IEnumerable<ReadOnlyMemory<char>> src,
            bool removeHashtags, bool removeUserMentions, bool removeUrls)
        {
            return CleanTweets(src, removeHashtags, removeUserMentions, removeUrls, false, false);
        }

        public static IEnumerable<ReadOnlyMemory<char>> CleanTweets(this IEnumerable<ReadOnlyMemory<char>> src,
            bool removeHashtags, bool removeUserMentions, bool removeUrls, bool removeRetweetInfo, bool cleanHashtags)
        {
            if (removeHashtags && cleanHashtags)
                throw new Exception("invalid arguments, hashtags shall be removed and cleaned, not possible");

            const string httpString = "http://";
            const string httpsString = "https://";

            var idx = -1;
            var startsWithRetweetInfo = false;
            foreach (var sMem in src)
            {
                idx++;
                var s = sMem.Span;
                if (s.IsEmpty || s.Length == 0)
                {
                    continue;
                }
                if (idx == 1 && startsWithRetweetInfo && s[0] == '@')
                    continue;
                if (s[^1] == '…')
                {
                    //tweets are sometimes capped inside words, eg. resol…
                    //we still want to indicate that sentence continues, but replace this with just
                    //the dots to avoid high-ranked "unique" words
                    yield return "…".AsMemory();
                    continue;
                }

                var chr = s[0];
                switch (chr)
                {
                    case '#':
                        if (removeHashtags)
                            continue;
                        if (cleanHashtags)
                        {
                            if (s.Length > 1)
                                yield return sMem[1..];
                            continue;
                        }
                        break;
                    case '@':
                        if (removeUserMentions)
                            continue;
                        break;
                    case 'h':
                        if (removeUrls)
                        {
                            if (s.StartsWith("http", StringComparison.Ordinal)
                                && s.Contains("://", StringComparison.Ordinal)
                                || s[^1] == '…' && s.Length < 7 && s[1] == 't' &&
                                (s[..^1].SequenceEqual(httpString.AsSpan(0, s.Length - 1))
                                || s[..^1].SequenceEqual(httpsString.AsSpan(0, s.Length - 1))))

                                continue;
                        }
                        break;
                    case 'R':
                    case 'r':
                        if (removeRetweetInfo && idx == 0 &&
                            (s.SequenceEqual("RT") || s.SequenceEqual("rt")))
                        {
                            startsWithRetweetInfo = true;
                            continue;
                        }
                        break;
                }
                yield return sMem;
            }
        }

        public static IEnumerable<ReadOnlyMemory<char>> TweetToWordsLowercase(this string src,
            bool removeHashtags = false, bool removeUserMentions = false, bool removeUrls = false, bool removeRetweetInfo = false, bool cleanHashtags = false)
        {
            return src.ToLowerInvariant().SplitSpaces()
                .CleanTweets(removeHashtags, removeUserMentions, removeUrls, removeRetweetInfo, cleanHashtags)
                .Tokenize();
        }

        public static IEnumerable<ReadOnlyMemory<char>> Tokenize(this string src)
        {
            return src.SplitSpaces().Tokenize();
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static bool IsVariationSelector(char chr)
        {
            return chr >= 65024 && chr <= 65039;
        }

        internal const char HIGH_SURROGATE_START = '\ud800';
        internal const char LOW_SURROGATE_START = '\udc00';
        internal const int UNICODE_PLANE01_START = 0x10000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ConvertToUtf32(ReadOnlySpan<char> s, int index)
        {
            var temp1 = s[index] - HIGH_SURROGATE_START;
            if (temp1 >= 0 && temp1 <= 0x3ff)
            {
                var temp2 = s[index + 1] - LOW_SURROGATE_START;
                if (temp2 >= 0 && temp2 <= 0x3ff)
                {
                    // Found a low surrogate.
                    return (temp1 * 0x400) + temp2 + UNICODE_PLANE01_START;
                }
            }
            return s[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static IEnumerable<ReadOnlyMemory<char>> Tokenize(this IEnumerable<ReadOnlyMemory<char>> src)
        {
            if (!SlicingListBag.TryTake(out var slicingList))
                slicingList = new FastClearList<(int startIdx, int length)>();
            try
            {
                foreach (var sMem in src)
                {
                    var s = sMem.Span;
                    var startIdx = -1;
                    //we need workaround of temporary list of "spans" because
                    //Span<T> is stack-only and thus not permitted across yield returns
                    slicingList.Clear();
                    for (int i = 0; i < s.Length; i++)
                    {
                        var chr = s[i];
                        if (IsVariationSelector(chr) || chr == '\u200D' /*zero-width joiner*/)
                            continue;//make sure that new token does not start with variation selector

                        if (chr >= 8205 && chr <= 12953 || chr >= 55356 && chr <= 56128)
                        {
                            //could be double- OR single-char emoji
                            //sometimes single-char emoji can still be transformed with variation selector afterwards
                            if (chr >= 55356 && i >= s.Length - 1)
                            {
                                //we fail gracefully to also process malformed text
                                break;
                            }
                            //warning: ConvertToUtf32 fails if current char is low surrogate char or if no char is following
                            var val = ConvertToUtf32(s, i);
                            if (EmojisUtf32Set.Contains(val))
                            {
                                //it is an emoji
                                if (startIdx >= 0)
                                {
                                    slicingList.Add((startIdx, i- startIdx));
                                }
                                //grapheme element / text element can be very long with modifiers etc
                                //for now, we first convert token to string
                                //in later versions of .NET, GetNextTextElement will have native support for Spans
                                var w = StringInfo.GetNextTextElement(sMem[i..].ToString(), 0);
                                slicingList.Add((i, w.Length));
                                i += w.Length - 1;
                                startIdx = -1;
                                continue;
                            }
                        }
                        if (PunctuationCharsSet.Contains(chr))
                        {
                            if (i > 0 && i < s.Length - 1 && Array.IndexOf(SpecialPunctuationChars, chr) != -1
                                && char.IsLetterOrDigit(s[i - 1]) && char.IsLetterOrDigit(s[i + 1]))
                            {
                                //this is a bit dirty and focused on the English language
                                //special case of long-running or didn't -> do not introduce new token here
                                if (s[i + 1] != 's' || s.Length != i + 2 && char.IsLetterOrDigit(s[i + 2]))
                                    continue;
                            }

                            if (startIdx >= 0)
                            {
                                slicingList.Add((startIdx, i - startIdx));
                            }
                            slicingList.Add((i, 1));
                            startIdx = -1;
                            continue;
                        }

                        if (startIdx == -1)
                        {
                            startIdx = i;
                        }
                    }

                    for (int i = 0; i < slicingList.Count; i++)
                    {
                        var range = slicingList[i];
                        yield return sMem.Slice(range.startIdx, range.length);
                    }

                    if (startIdx == 0)
                        yield return sMem;
                    else if (startIdx > 0)
                        yield return sMem.Slice(startIdx, sMem.Length - startIdx);
                }

            }
            finally
            {
                SlicingListBag.Add(slicingList);
            }
        }


        public static IEnumerable<ReadOnlyMemory<char>> ToLowerInvariant(this IEnumerable<ReadOnlyMemory<char>> src)
        {
            foreach (var s in src)
            {
                var span = s.Span;
                var hasUpper = false;
                for (var i = 0; i < span.Length; i++)
                {
                    var chr = span[i];
                    if (!char.IsUpper(chr)) continue;
                    hasUpper = true;
                    break;
                }

                yield return hasUpper ? s.ToString().ToLowerInvariant().AsMemory() : s;
            }
        }


        public static IEnumerable<ReadOnlyMemory<char>> RemovePunctuationChars(this IEnumerable<ReadOnlyMemory<char>> src)
        {
            foreach (var s in src)
            {
                if (s.Length == 1 && PunctuationCharsSet.Contains(s.Span[0]))
                    continue;
                yield return s;
            }

        }

        public static IEnumerable<ReadOnlyMemory<char>> RemoveShortWords(this IEnumerable<ReadOnlyMemory<char>> src, int minNoChars)
        {
            foreach (var s in src)
            {
                if (s.Length < minNoChars)
                    continue;

                yield return s;
            }

        }

        public static IEnumerable<string> FilterStopWords(this IEnumerable<string> src, ISet<string> stopWords)
        {
            foreach (var s in src)
            {
                //ToLowerInvariant only allocates new string if there is indeed an uppercase character in the string
                if (stopWords.Contains(s.ToLowerInvariant()))
                    continue;
                yield return s;
            }
        }


    }
}
