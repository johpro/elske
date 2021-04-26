/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using ElskeLib.Model;
using ElskeLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ElskeLib.Tests.Utils
{
    [TestClass()]
    public class StopWordsTests
    {
        

        [TestMethod]
        public void ListFrequentEnglishWordsTest()
        {

            Trace.WriteLine("en-news:");
            var elske = KeyphraseExtractor.FromFile("../../../../../models/en-news.elske");
            
            Trace.WriteLine("\r\nregular stop words:");
            ListFrequentWords(elske, StopWords.EnglishStopWords);

            Trace.WriteLine("\r\nminimal stop words:");
            ListFrequentWords(elske, StopWords.EnglishStopWordsMinimal);

            Trace.WriteLine("\r\n\r\nen-twitter:");

            elske = KeyphraseExtractor.FromFile("../../../../../models/en-twitter.elske");

            Trace.WriteLine("\r\nregular stop words:");
            ListFrequentWords(elske, StopWords.EnglishStopWords);

            Trace.WriteLine("\r\nminimal stop words:");
            ListFrequentWords(elske, StopWords.EnglishStopWordsMinimal);
        }

        private static void ListFrequentWords(KeyphraseExtractor elske, string[] stopWords)
        {
            var frequentWords = elske.ReferenceCounts
                .DocCounts.WordCounts.OrderByDescending(p => p.Value);
            var stopWordsSet = stopWords
                .Concat(StopWords.PunctuationStopWords)
                .Concat(StopWords.DigitsStopWords)
                .Select(w => elske.ReferenceIdxMap.WordToIdx.GetValueOrDefault(w, -1))
                .Where(w => w >= 0)
                .ToHashSet();
            foreach (var wTuple in frequentWords
                .Where(p => !stopWordsSet.Contains(p.Key)).Take(100))
            {
                Trace.WriteLine($"{elske.ReferenceIdxMap.IdxToWord[wTuple.Key]} [{wTuple.Value}]");
            }
        }

    }
}