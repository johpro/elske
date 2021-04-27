/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

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
            '‛', '‟', '‹', '›', '‘', '’', '’', '\u201A', '_', '^',
            '/', '\\', '[', ']', '“', '”', '{', '}', '—', '–', '-', '\'', '*', '=' //* important for wiki and *bold* text, = for wiki header markup
        };


        /// <summary>
        /// Characters that may be part of a word (e.g, didn't), but are also punctuation chars or quotation marks
        /// </summary>
        public static readonly char[] SpecialPunctuationChars = { '-', '’', '\'' };

        public static readonly HashSet<char> PunctuationCharsSet = new(PunctuationChars);

        
        public static readonly HashSet<int> EmojisUtf32Set = new(EmojiHelper.ListOfEmojisUtf32);



        public static IEnumerable<string> SplitSpaces(this string src)
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
                        yield return src.AsSpan(startIdx, i - startIdx).ToString();
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
                yield return src;
            }

            if (startIdx > 0)
            {
                yield return src.AsSpan(startIdx, src.Length - startIdx).ToString();
            }
        }


        public static IEnumerable<string> CleanTweets(this IEnumerable<string> src,
            bool removeHashtags, bool removeUserMentions, bool removeUrls)
        {
            return CleanTweets(src, removeHashtags, removeUserMentions, removeUrls, false, false);
        }

        public static IEnumerable<string> CleanTweets(this IEnumerable<string> src,
            bool removeHashtags, bool removeUserMentions, bool removeUrls, bool removeRetweetInfo, bool cleanHashtags)
        {
            if (removeHashtags && cleanHashtags)
                throw new Exception("invalid arguments, hashtags shall be removed and cleaned, not possible");

            const string httpString = "http://";
            const string httpsString = "https://";

            var idx = -1;
            var startsWithRetweetInfo = false;
            foreach (var s in src)
            {
                idx++;



                if (string.IsNullOrEmpty(s))
                {
                    yield return s;
                    continue;
                }

                if (idx == 1 && startsWithRetweetInfo && s[0] == '@')
                    continue;

                if (s[^1] == '…')
                {
                    //tweets are sometimes capped inside words, eg. resol…
                    //we still want to indicate that sentence continues, but replace this with just
                    //the dots to avoid high-ranked "unique" words
                    yield return "…";
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
                            if(s.Length > 1)
                                yield return s.Substring(1);
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
                                (s.AsSpan(0, s.Length - 1).SequenceEqual(httpString.AsSpan(0, s.Length - 1))
                                || s.AsSpan(0, s.Length - 1).SequenceEqual(httpsString.AsSpan(0, s.Length - 1))))

                                continue;
                        }

                        break;
                    case '&':
                        switch (s)
                        {
                            case "&amp":
                            case "&amp;":
                                yield return "&";
                                continue;
                            case "&quot":
                            case "&quot;":
                                yield return "\"";
                                continue;
                            case "&lt":
                            case "&lt;":
                                yield return "<";
                                continue;
                            case "&gt":
                            case "&gt;":
                                yield return ">";
                                continue;

                        }

                        break;
                    case 'R':
                    case 'r':
                        if (removeRetweetInfo && idx == 0 && 
                            (s == "RT" || s == "rt"))
                        {
                            startsWithRetweetInfo = true;
                            continue;
                        }
                        break;

                }

                yield return s;
            }
        }

        public static IEnumerable<string> TweetToWordsLowercase(this string src,
            bool removeHashtags = false, bool removeUserMentions = false, bool removeUrls = false, bool removeRetweetInfo = false, bool cleanHashtags = false)
        {
            return src.SplitSpaces()
                .CleanTweets(removeHashtags, removeUserMentions, removeUrls, removeRetweetInfo, cleanHashtags)
                .Tokenize().ToLowerInvariant();
        }

        public static IEnumerable<string> Tokenize(this string src)
        {
            return src.SplitSpaces().Tokenize();
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static bool IsVariationSelector(char chr)
        {
            return chr >= 65024 && chr <= 65039;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static IEnumerable<string> Tokenize(this IEnumerable<string> src)
        {
            foreach (var s in src)
            {
                var startIdx = -1;
                for (int i = 0; i < s.Length; i++)
                {
                    var chr = s[i];
                    if(IsVariationSelector(chr) || chr == '\u200D' /*zero-width joiner*/)
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
                        var val = char.ConvertToUtf32(s, i);
                        if (EmojisUtf32Set.Contains(val))
                        {
                            //it is an emoji
                            if (startIdx >= 0)
                            {
                                yield return s.AsSpan(startIdx, i - startIdx).ToString();
                            }

                            //grapheme element / text element can be very long with modifiers etc
                            var w = StringInfo.GetNextTextElement(s, i);
                            yield return w;
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
                            yield return s.AsSpan(startIdx, i - startIdx).ToString();
                        }

                        yield return chr.ToString();
                        startIdx = -1;
                        continue;
                    }

                    if (startIdx == -1)
                    {
                        startIdx = i;
                    }
                }

                if (startIdx == 0)
                    yield return s;

                if (startIdx > 0)
                    yield return s.AsSpan(startIdx, s.Length - startIdx).ToString();
            }
        }


        public static IEnumerable<string> ToLowerInvariant(this IEnumerable<string> src)
        {
            foreach (var s in src)
            {
                yield return s.ToLowerInvariant();
            }
        }

        public static IEnumerable<string> RemovePunctuationChars(this IEnumerable<string> src)
        {
            foreach (var s in src)
            {
                if (s.Length == 1 && PunctuationCharsSet.Contains(s[0]))
                    continue;

                yield return s;
            }

        }

        public static IEnumerable<string> RemoveShortWords(this IEnumerable<string> src, int minNoChars)
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
