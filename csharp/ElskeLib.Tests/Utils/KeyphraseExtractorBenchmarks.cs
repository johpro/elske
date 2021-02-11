/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElskeLib.Model;
using ElskeLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ElskeLib.Tests.Utils
{
    [TestClass()]
    public class KeyphraseExtractorBenchmarks
    {
        
        [TestMethod]
        public void PerformDataSetBenchmarks()
        {
            PerformBenchmark(EvalDataSet.SemEval2010);
            PerformBenchmark(EvalDataSet.Krapivin);
            PerformBenchmark(EvalDataSet.Nus);
            PerformBenchmark(EvalDataSet.Inspec);
        }

        public void PerformBenchmark(EvalDataSet setMode)
        {

            Trace.WriteLine($"\r\n====== {setMode} ======\r\n");


            const int numTopPhrases = 10;

            var extractorStemmed = InitializeEvalKeyphraseExtractor(setMode);
            extractorStemmed.MinNumCharacters = 3;
            extractorStemmed.OverhangTfIdfThresholdFactor = .1;

            var testStemmed = setMode switch
            {
                EvalDataSet.SemEval2010 => ReadSemEval2010(true),
                EvalDataSet.Inspec => ReadInspec(false),
                EvalDataSet.Krapivin => ReadKrapivin(),
                EvalDataSet.Nus => ReadNus(),
                _ => throw new ArgumentOutOfRangeException()
            };

            Trace.WriteLine(testStemmed.Count + " test items loaded");

            var stemmer = new PorterStemmer();
            for (var i = 0; i < testStemmed.Count; i++)
            {
                var it = testStemmed[i];

                for (int j = 0; j < it.Keyphrases.Length; j++)
                {
                    var key = it.Keyphrases[j];
                    if (setMode == EvalDataSet.SemEval2010 && key.Contains('+'))
                    {
                        key = string.Join("+", key.Split('+').Select(k => stemmer.Stem(k)));
                    }
                    else
                    {
                        key = stemmer.Stem(key);
                    }
                    it.Keyphrases[j] = key;
                    //if (it.Keyphrases[j].Any(char.IsDigit))  Trace.WriteLine(it.Keyphrases[j]);
                }

                var stemmedText = stemmer.Stem(it.Text);

                var presentKeys = it.Keyphrases.Where(k =>
                    IsProperlyContained(k, stemmedText)
                ).ToList();

                if (setMode == EvalDataSet.SemEval2010)
                {
                    presentKeys.Clear();
                    foreach (var k in it.Keyphrases)
                    {
                        if (k.Contains('+'))
                        {
                            var split = k.Split('+');
                            var isContained1 = IsProperlyContained(split[0], stemmedText);
                            var isContained2 = IsProperlyContained(split[1], stemmedText);
                            if (isContained1 && isContained2)
                                presentKeys.Add(k);
                            else if (isContained1)
                                presentKeys.Add(split[0]);
                            else if (isContained2)
                                presentKeys.Add(split[1]);
                        }
                        else
                        {
                            if (IsProperlyContained(k, stemmedText))
                                presentKeys.Add(k);
                        }
                    }
                }

                testStemmed[i] = new AnnotatedText(it.Text, presentKeys.Distinct().ToArray());

            }

            testStemmed = testStemmed.Where(it => it.Keyphrases.Length > 0).ToList();

            Trace.WriteLine(testStemmed.Count + " test items with <present> keywords");

            var testStemmedResults = new string[testStemmed.Count][];

            Parallel.For(0, testStemmedResults.Length, i =>
            {
                AnnotatedText data;
                int[] tokens;
                lock (testStemmedResults)
                {
                    data = testStemmed[i];
                    tokens = extractorStemmed.ReferenceIdxMap
                        .TokensToIndexes(data.Text.Tokenize().ToLowerInvariant()).ToArray();
                }

                var phrases =
                    extractorStemmed.ExtractPhrases(new List<int[]> { tokens }, numTopPhrases);
                var res = phrases.Select(p => p.Phrase).ToArray();

                lock (testStemmedResults)
                    testStemmedResults[i] = res;

            });

            Trace.WriteLine(setMode + " - test set, stemmed - top 10 results:\r\n");

            var testNo = 45;

            Trace.WriteLine($"test text:\r\n{testStemmed[testNo].Text}\r\n");

            Trace.WriteLine("ground truth: " + string.Join(",", testStemmed[testNo].Keyphrases));

            Trace.WriteLine("results: " + string.Join(",", testStemmedResults[testNo]));


            foreach (var noKeywords in new[] { 5, 10, 15, 20 })
            {
                Trace.WriteLine("\r\n========================");
                Trace.WriteLine("P/R/F@" + noKeywords);

                var evals = new List<EvalResults>();

                for (int i = 0; i < testStemmedResults.Length; i++)
                {
                    var res = testStemmedResults[i].Take(noKeywords).ToArray();
                    var truth = testStemmed[i].Keyphrases;

                    for (int j = 0; j < res.Length; j++)
                    {
                        res[j] = stemmer.Stem(res[j]);
                    }



                    var eval = EvalResults.FromStringSet(truth, res);
                    //Trace.WriteLine(eval);
                    evals.Add(eval);
                }

                var prec = evals.Average(it => it.Precision);
                var rec = evals.Average(it => it.Recall);

                Trace.WriteLine(new EvalResults(prec, rec));
            }


        }


        static bool IsProperlyContained(string token, string text)
        {
            return text.Contains($" {token} ", StringComparison.Ordinal) ||
                   text.StartsWith(token + " ") ||
                   text.EndsWith(" " + token);
        }

        [TestMethod]
        public void PorterStemmerTest()
        {
            var text = File.ReadAllText("../../../../../datasets/SemEval2010/task05-TRAIN/train/C-71.txt.final");

            var stemmer = new PorterStemmer();
            Trace.WriteLine(stemmer.Stem(text, true));
        }

        public enum EvalDataSet { SemEval2010, Inspec, Krapivin, Nus }



        public static KeyphraseExtractor InitializeEvalKeyphraseExtractor(EvalDataSet set)
        {
            var dataset = set switch
            {
                EvalDataSet.SemEval2010 => ReadSemEval2010(true),
                EvalDataSet.Inspec => ReadInspec(true),
                EvalDataSet.Krapivin => ReadKrapivin(),
                EvalDataSet.Nus => ReadNus(),
                _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
            };

            var collection = new List<int[]>();
            var idxMap = new WordIdxMap();

            foreach (var annotatedText in dataset)
            {
                collection.Add(idxMap.TokensToIndexes(annotatedText.Text.Tokenize()
                    .ToLowerInvariant()).ToArray());
            }

            var extractor = new KeyphraseExtractor
            {
                ReferenceIdxMap = idxMap,
                ReferenceCounts = CorpusCounts.GetCounts(collection),
                StopWords = StopWords.EnglishStopWords
                    .Concat(StopWords.DigitsStopWords)
                    .Concat(StopWords.PunctuationStopWords).ToArray()
            };

            extractor.ReferenceDocuments = new BigramDocumentIndex();
            for (var i = 0; i < collection.Count; i++)
            {
                var doc = collection[i];
                extractor.ReferenceDocuments.AddDocument(doc);
            }

            return extractor;
        }


        private static List<AnnotatedText> ReadSemEval2010(bool useTest)
        {
            var dir = new DirectoryInfo(useTest
                ? "../../../../../datasets/SemEval2010/Task05-test+Keys/test"
                : "../../../../../datasets/SemEval2010/task05-TRAIN/train");

            var annotationsFn = useTest
                ? "../../../../../datasets/SemEval2010/Task05-test+Keys/test.combined.stem.final"
                : "../../../../../datasets/SemEval2010/task05-TRAIN/train/train.combined.final";

            var annotations = File.ReadLines(annotationsFn)
                .Select(l => l.Split(':'))
                .ToDictionary(l => l[0].Trim(), l => l[1].Trim().Split(','));

            var files = dir.GetFiles("*.txt.final");

            return files.AsParallel().Select(file =>
            {
                var lines = File.ReadAllLines(file.FullName);
                var title = lines[0] + " " + (char.IsLetter(lines[1][0]) ? lines[1] : "");
                var text = title + ".\r\n" +
                           string.Join("\r\n", lines
                               .SkipWhile(l => !l.Contains("ABSTRACT", StringComparison.OrdinalIgnoreCase))
                               .Skip(1)
                               .TakeWhile(l => !l.Contains("ABSTRACT", StringComparison.OrdinalIgnoreCase) &&
                                               !l.Contains("Categories and Subject Descriptors",
                                                   StringComparison.OrdinalIgnoreCase)
                                               && !l.StartsWith("1. ")));
                var key = file.Name.Split('.')[0];
                var keywords = annotations[key];

                text = string.Join(" ", text.Tokenize().ToLowerInvariant());

                return new AnnotatedText(text, keywords);
            }).ToList();

        }


        private static List<AnnotatedText> ReadInspec(bool useTest)
        {
            var dir = new DirectoryInfo(useTest
                ? "../../../../../datasets/Hulth2003/Test"
                : "../../../../../datasets/Hulth2003/Training");

            var files = dir.GetFiles("*.abstr");


            return files.Select(file =>
            {
                var text = File.ReadAllText(file.FullName);

                var labelsFn = file.FullName.Substring(0, file.FullName.Length - 5) + "uncontr";

                var keywords = File.ReadAllText(labelsFn)
                    .Replace("\r\n\t", " ")
                    .Replace("\r\n", " ")
                    .Split(';').Select(s => s.Trim(' ', '\t', '\r', '\n').ToLowerInvariant())
                    .Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

                text = string.Join(" ", text.Tokenize().ToLowerInvariant());

                return new AnnotatedText(text, keywords);
            }).ToList();

        }
        private static List<AnnotatedText> ReadKrapivin()
        {
            var dir = new DirectoryInfo(@"../../../../../datasets/Krapivin");

            var files = dir.GetFiles("*.txt");
            var src = files.OrderBy(f => f.Name).AsEnumerable();

            return src.AsParallel().Select(file =>
            {
                var text = string.Join("\r\n", File.ReadLines(file.FullName)
                    .TakeWhile(l => !l.StartsWith("--B"))
                    .Where(l => !l.StartsWith("-"))
                    .Select(l => l.StartsWith("Abstract", StringComparison.OrdinalIgnoreCase) ? l[8..] : l));

                var labelsFn = file.FullName[0..^3] + "key";

                var keywords = File.ReadLines(labelsFn)
                    .Select(s => s.Trim(' ', '\t', '\r', '\n').ToLowerInvariant())
                    .Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

                text = string.Join(" ", text.Tokenize().ToLowerInvariant());

                return new AnnotatedText(text, keywords);
            }).ToList();

        }

        private static List<AnnotatedText> ReadNus()
        {
            var dir = new DirectoryInfo(@"../../../../../datasets/Nguyen2007/data");

            var files = dir.GetFiles("*.txt", SearchOption.AllDirectories);

            var src = files.OrderBy(f => f.Name).AsEnumerable();

            return src.AsParallel().Select(file =>
            {
                var textLines = File.ReadAllLines(file.FullName);

                //Meng et al. use only title and abstract for evaluation
                var text =
                    string.Join("\r\n", textLines
                        .TakeWhile(l => !string.IsNullOrWhiteSpace(l)
                                        && l.ToLowerInvariant().Trim() != "introduction"
                                        && l.ToLowerInvariant().Trim() != "categories and subject descriptors"
                                        && l.ToLowerInvariant().Trim() != "categories & subject descriptors"
                                        )
                        .Where(l => l.ToLowerInvariant().Trim() != "abstract"));

                var labelsFn = file.FullName.Substring(0, file.FullName.Length - 3) + "kwd";
                if (!File.Exists(labelsFn))
                    return new AnnotatedText();

                if (string.IsNullOrWhiteSpace(labelsFn) || !File.Exists(labelsFn))
                    throw new Exception($"problem with entry {file.Name}");

                var keywords = File.ReadLines(labelsFn).Select(
                        s => s.Trim(' ', '\t', '\r', '\n').ToLowerInvariant())
                    .Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

                text = string.Join(" ", text.Tokenize().ToLowerInvariant());

                return new AnnotatedText(text, keywords);
            }).Where(it => it.Text != null).ToList();

        }

        readonly struct AnnotatedText
        {
            public AnnotatedText(string text, string[] keyphrases)
            {
                Text = text;
                Keyphrases = keyphrases;
            }

            public string Text { get; }
            public string[] Keyphrases { get; }
        }

        readonly struct EvalResults
        {
            public EvalResults(double precision, double recall)
            {
                Precision = precision;
                Recall = recall;

                FVal = (precision + recall) <= double.Epsilon ? 0 : 2 * precision * recall / (precision + recall);
            }

            public double Precision { get; }
            public double Recall { get; }
            public double FVal { get; }

            public static EvalResults FromStringSet(string[] truth, string[] calculated)
            {

                var noCorrect = 0; //truth.Intersect(calculated).Count();

                foreach (var s in calculated)
                {
                    //semeval ground truth has sometimes alternatives for keyphrases, hence the effort here
                    if (truth.Any(t => t == s
                                       || t.Contains(s + "+", StringComparison.Ordinal)
                                       || t.Contains("+" + s, StringComparison.Ordinal)))
                        noCorrect++;
                }

                var prec = noCorrect / (double)calculated.Length;
                var rec = noCorrect / (double)truth.Length;
                return new EvalResults(prec, rec);
            }

            public override string ToString()
            {
                return $"P {Precision:N3} R {Recall:N3} F {FVal:N3}";
            }
        }




    }
}