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
using ElskeLib.Model;
using ElskeLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ElskeLib.Tests.Utils
{
    [TestClass()]
    public class KeyphraseExtractorTests
    {

        [TestMethod()]
        public void CreateFromFolderTest()
        {
            const string dir = "../../../../../datasets/SemEval2010/Task05-test+Keys/test";
            const string testFn = dir + "/H-16.txt.final";
            var elske = KeyphraseExtractor.CreateFromFolder(dir);
            Assert.AreEqual(100, elske.ReferenceCounts.DocCounts.NumDocuments);
            var phrases = elske.ExtractPhrases(File.ReadAllText(testFn), 10);
            Assert.AreEqual(10, phrases.Count);

            foreach (var p in phrases)
            {
                Trace.WriteLine(p.ToString());
            }

            var tmpFn = Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                elske.ToFile(tmpFn);
                elske = KeyphraseExtractor.FromFile(tmpFn);
                Assert.AreEqual(100, elske.ReferenceCounts.DocCounts.NumDocuments);
                var phrases2 = elske.ExtractPhrases(File.ReadAllText(testFn), 10);
                Assert.AreEqual(10, phrases.Count);
                for (int i = 0; i < phrases.Count; i++)
                {
                    Assert.AreEqual(phrases[i].Phrase, phrases2[i].Phrase);
                }

            }
            finally
            {
                File.Delete(tmpFn);
            }
        }



        [TestMethod]
        public void ExtractPhrasesNewsArticleTest()
        {
            var article =
                "Three people seriously injured jumping into sea at Dorset beach\r\n\r\nThree people have been seriously injured jumping off cliffs into the sea at a beach in Dorset.\r\nAir ambulances landed at the scene at Durdle Door on Saturday afternoon, and police are now asking people to stay away from the popular tourist spot, near Lulworth.\r\n\r\nA post on Poole police’s Facebook page said people had been jumping from the arch of rocks at the beach, and videos posted on other social media networks show people climbing and making the 200ft leap.\r\nThe well-known limestone arch at Durdle Door beach, Dorset – and people enjoying the sand and sea earlier on Saturday.\r\n\r\nImages posted on social media by Purbeck police show helicopters landing on the sand and crowds leaving the beach en masse as the area was evacuated.\r\n\r\nHM Coastguard and the RNLI are helping to clear the area after police were called at around 3.45pm.\r\n\r\nCh Insp Claire Phillips, of Dorset police, said: “We have had to close the beach at Durdle Door to allow air ambulances to land. As a result, we are evacuating the beach and the surrounding cliff area.\r\n\r\n“I am urging people to leave the area to enable emergency services to treat the injured people.”\r\n\r\nPictures taken earlier on Saturday showed the beach busy as the public were reminded to practise physical distancing in the good weather following the relaxation of coronavirus lockdown restrictions.";

            var elske = KeyphraseExtractor.FromFile("../../../../../models/en-news.elske");

            var phrases = elske.ExtractPhrases(article, 10);

            foreach (var phraseResult in phrases)
            {
                Trace.WriteLine($"{phraseResult.Phrase}\t[{phraseResult.TfIdf}]");
            }


            Trace.WriteLine("\r\nARTICLE:\r\n");
            Trace.WriteLine(article);

            Assert.AreEqual(10, phrases.Count);
            Assert.AreEqual(10, phrases.Select(p => p.Phrase).Distinct().Count());
            Assert.IsTrue(phrases.Any(p => p.Phrase == "dorset"));
            Assert.IsTrue(phrases.Any(p => p.Phrase == "beach"));
            Assert.IsTrue(phrases.Any(p => p.Phrase == "durdle door"));
        }


        [TestMethod]
        public void IncompletePhrasesTest()
        {
            var elske = KeyphraseExtractor.FromFile("../../../../../models/en-news.elske");

            var sentence =
                elske.ReferenceIdxMap.DocumentToIndexes("say no to bigotry stop bigotry now actnow").ToArray();

            var sentences = new List<int[]>();
            for (int n = 100; n >= 1; n -= 20)
            {
                for (int i = 0; i < n; i++)
                {
                    sentences.Add(sentence);
                }

                if (sentence.Length <= 1)
                    break;

                sentence = sentence.Take(sentence.Length - 1).ToArray();
            }


            var phrases = elske.ExtractPhrases(sentences, 5);

            foreach (var phraseResult in phrases)
            {
                Trace.WriteLine($"{phraseResult.Phrase}\t[{phraseResult.TfIdf}]");
            }
        }

        [TestMethod()]
        public void ExtractPhrasesTest()
        {
            const bool useUniqueTweetsOnly = false;


            var fn = @"../../../../../datasets/twitter/20-02-12__tweets_en.csv.gz";

            var extractor = KeyphraseExtractor.FromFile("../../../../../models/en-twitter.elske");

            var avgTweetsPerHour = 15_000_000 / 24;
            var noLocalTweets = 1_000_000; // 5 * avgTweetsPerHour / 2;
            var localSentences = new int[noLocalTweets][];

            var hashSet = new HashSet<WordSequence>();
            var m = 0;
            var added = 0;
            foreach (var l in FileReader.ReadLines(fn))
            {
                m++;
                if (m >= 6 * avgTweetsPerHour)
                {

                    var text = l.Substring(l.LastIndexOf('\t') + 1);

                    var arr = extractor.ReferenceIdxMap.TokensToIndexes(text.TweetToWordsLowercase(removeUrls: true, cleanHashtags: true, removeRetweetInfo: true).RemovePunctuationChars()).ToArray();
                    if (useUniqueTweetsOnly && !hashSet.Add(WordSequence.CreateWithReference(arr)))
                        continue;

                    localSentences[added] = arr;
                    added++;
                    if (added >= localSentences.Length)
                        break;
                }


            }


            Trace.WriteLine($"{localSentences.Length} sentences in local collection, {added} added");
            Trace.WriteLine("top 50");

            static void ShowRes(KeyphraseExtractor extractor, int[][] arr)
            {

                Trace.WriteLine("\r\n==========================\r\n");
                foreach (var noKeyWords in new[] { 5, 10, 15, 20, 25, 50 })
                {

                    var res = extractor.ExtractPhrases(arr, noKeyWords);

                    Trace.WriteLine(noKeyWords + ":");
                    Trace.WriteLine(string.Join(", ", res.Take(noKeyWords)
                        .Select(r => $"{r.Phrase} ({r.TermFrequency})")));

                }
            }

            ShowRes(extractor, localSentences);

            Trace.WriteLine("\r\n======= TF-IDF =========");

            extractor.UsePfIdf = false;
            ShowRes(extractor, localSentences);

            extractor.Mode = KeyphraseExtractor.ExtractingMode.UnigramsOnly;
            ShowRes(extractor, localSentences);


            Trace.WriteLine("\r\n======= PF-IDF =========");
            extractor.UsePfIdf = true;

            ShowRes(extractor, localSentences);

        }


        [TestMethod()]
        public void ExtractPhrasesOnlineTest()
        {
            var fn = @"../../../../../datasets/twitter/20-02-12__tweets_en.csv.gz";

            var extractor = KeyphraseExtractor.FromFile("../../../../../models/en-twitter.elske");

            var avgTweetsPerHour = 15_000_000 / 24;
            var noLocalTweets = 1_000_000;

            var m = 0;

            var docs = FileReader.ReadLines(fn)
                .Skip(6 * avgTweetsPerHour)
                .Take(noLocalTweets)
                .Select(l =>
                {
                    m++; //to check number of enumerations later on
                    return l.Substring(l.LastIndexOf('\t') + 1);
                });
            
            var res = extractor.ExtractPhrases(docs, 50, true);

            Trace.WriteLine(string.Join(", ", res
                .Select(r => $"{r.Phrase} ({r.TermFrequency})")));
            
            Assert.AreEqual(50, res.Count);
            Assert.AreEqual(2*noLocalTweets, m);

        }

    }
}