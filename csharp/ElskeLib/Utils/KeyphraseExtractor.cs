﻿/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ElskeLib.Model;

namespace ElskeLib.Utils
{
    /// <summary>
    /// ELSKE: extract (possibly longer) keyphrases and keywords
    /// from individual documents as well as document collections.
    /// </summary>
    public class KeyphraseExtractor
    {

        /// <summary>
        /// Term and document frequencies for calculating the IDF
        /// </summary>
        public CorpusCounts ReferenceCounts { get; set; }

        /// <summary>
        /// The collection of indexed documents for inferring the inverse document frequency of phrases (>= 2 terms).
        /// Can be null, in which case the frequency will be estimated using the bigram doc counts in ReferenceCounts.
        /// </summary>
        public BigramDocumentIndex ReferenceDocuments { get; set; }
        /// <summary>
        /// Mapping of vocabulary to integers, must be compatible with reference counts
        /// </summary>
        public WordIdxMap ReferenceIdxMap { get; set; }

        /// <summary>
        /// list of stop words to be ignored, must be compatible with applied tokenization
        /// </summary>
        public string[] StopWords
        {
            get => _stopWords;
            set
            {
                _stopWords = value;
                //we need to reinitialize hash set on next use
                _stopWordsSet = null;
            }
        }

        /// <summary>
        /// Threshold that determines how unique slightly phrase variants must be to be retained.
        /// The higher the number, the lower the number of variants.
        /// </summary>
        public double OverhangTfIdfThresholdFactor { get; set; } = 0.1;

        /// <summary>
        /// Minimum number of characters at least one word in the extracted keyphrase has to have
        /// </summary>
        public int MinNumCharacters { get; set; }

        /// <summary>
        /// Additional parameters that specify how often certain terms have to appear in the source or in the
        /// reference collection such that they are eligible as keywords/keyphrases.
        /// </summary>
        public CountRequirementOptions CountRequirements { get; set; } = new();

        /// <summary>
        /// Maximum sequence length of a keyphrase
        /// </summary>
        public int MaxNumWords { get; set; } = 40;

        /// <summary>
        /// Use adjusted phrase frequency instead of the plain occurrence/term frequency.
        /// Set to true if you want to extract keyphrases from document collections and not only individual documents.
        /// </summary>
        public bool UsePfIdf { get; set; } = true;

        public bool IsDebugTextOutputEnabled { get; set; }
        public bool IsDebugStopwatchEnabled { get; set; }

        /// <summary>
        /// Determines the mode of extraction, mainly for debugging purposes.
        /// For instance, if set to UnigramsOnly, only single-worded keywords will be extracted.
        /// </summary>
        public ExtractingMode Mode { get; set; } = ExtractingMode.Full;

        public enum ExtractingMode
        {
            UnigramsOnly = 1,
            UniAndBigramsOnly = 2,
            Phrases = 3,
            PhrasesRemoveLongerVariations = 4,
            Full = 5,
            DebugOnlyPhraseCandidates = 20
        }


        private HashSet<int> _stopWordsSet;
        private string[] _stopWords;

        /// <summary>
        /// pooling of dictionaries for vector generation
        /// </summary>
        private readonly ThreadLocal<Dictionary<int, int>> _dictLocal = new(() => new Dictionary<int, int>());

        private const string StorageMetaId = "keyphrase-extractor-meta.json";
        private const string StorageBlobId = "keyphrase-extractor.bin";

        private class StorageMeta
        {
            public string[] StopWords { get; set; }
            public double OverhangTfIdfThresholdFactor { get; set; }
            public int MinNumCharacters { get; set; }
            public int MaxNumWords { get; set; }
            public bool UsePfIdf { get; set; }
            public ExtractingMode Mode { get; set; }
            public int Version { get; set; } = 1;
        }
        /// <summary>
        /// Save extractor state to file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix">you can set a unique prefix if you want to store multiple states in the same file</param>
        public void ToFile(string fn, string entryPrefix = "")
        {
            ReferenceDocuments?.ToFile(fn, entryPrefix);
            ReferenceCounts?.ToFile(fn, entryPrefix);
            ReferenceIdxMap?.ToFile(fn, entryPrefix);

            var meta = new StorageMeta
            {
                Mode = Mode,
                MaxNumWords = MaxNumWords,
                MinNumCharacters = MinNumCharacters,
                OverhangTfIdfThresholdFactor = OverhangTfIdfThresholdFactor,
                StopWords = StopWords,
                UsePfIdf = UsePfIdf
            };
            var metaStr = JsonSerializer.Serialize(meta);
            using var stream = new FileStream(fn, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Update);
            zip.GetEntry(entryPrefix + StorageMetaId)?.Delete();
            var entry = zip.CreateEntry(entryPrefix + StorageMetaId);
            using (var entryStream = new StreamWriter(entry.Open()))
                entryStream.Write(metaStr);

        }


        /// <summary>
        /// Load extractor from file
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="entryPrefix"></param>
        /// <returns></returns>
        public static KeyphraseExtractor FromFile(string fn, string entryPrefix = "")
        {
            var extractor = new KeyphraseExtractor
            {
                ReferenceDocuments = BigramDocumentIndex.FromFile(fn, entryPrefix),
                ReferenceIdxMap = WordIdxMap.FromFile(fn, entryPrefix),
                ReferenceCounts = CorpusCounts.FromFile(fn, entryPrefix)
            };

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


            extractor.Mode = meta.Mode;
            extractor.MaxNumWords = meta.MaxNumWords;
            extractor.MinNumCharacters = meta.MinNumCharacters;
            extractor.OverhangTfIdfThresholdFactor = meta.OverhangTfIdfThresholdFactor;
            extractor.StopWords = meta.StopWords;
            extractor.UsePfIdf = meta.UsePfIdf;

            return extractor;
        }

        /// <summary>
        /// Initialize keyphrase extractor with required reference statistics based on provided documents.
        /// </summary>
        /// <param name="documents">Documents of the reference collection</param>
        /// <param name="settings">Parameters to influence tokenization and index building</param>
        /// <returns></returns>
        public static KeyphraseExtractor CreateFromDocuments(IEnumerable<string> documents, ElskeCreationSettings settings = null)
        {
            settings ??= new ElskeCreationSettings();
            var res = new KeyphraseExtractor
            {
                ReferenceIdxMap = new WordIdxMap { TokenizationSettings = settings.TokenizationSettings },
                ReferenceDocuments = settings.BuildReferenceCollection ? new BigramDocumentIndex() : null,
                IsDebugStopwatchEnabled = settings.IsDebugStopwatchEnabled,
                IsDebugTextOutputEnabled = settings.IsDebugTextOutputEnabled
            };
            var docs = documents.Select(doc =>
            {
                var tokens = res.ReferenceIdxMap.DocumentToIndexes(doc);
                res.ReferenceDocuments?.AddDocument(tokens);
                return tokens;
            });

            Stopwatch watch = null;
            if (settings.IsDebugStopwatchEnabled)
                watch = Stopwatch.StartNew();

            res.ReferenceCounts = settings.DoNotCountPairs
                ? CorpusCounts.GetDocTermCounts(docs)
                : CorpusCounts.GetDocCounts(docs);

            if (watch != null)
            {
                Trace.WriteLine($"{watch.Elapsed} for counting reference collection and creating index map"); watch.Restart();
            }

            if (settings.BuildReferenceCollection || settings.MinTermDocFrequency <= 1)
                return res;

            res.TruncateVocabulary(settings.MinTermDocFrequency); 

            if (watch != null)
            {
                Trace.WriteLine($"{watch.Elapsed} for cleaning ReferenceIdxMap and ReferenceCounts (now {res.ReferenceIdxMap.Count:N0} words)"); watch.Restart();
            }

            return res;
        }

        /// <summary>
        /// Remove tokens in the ReferenceIdxMap and ReferenceCounts that appear in less documents than the specified threshold (according to the
        /// ReferenceCounts of the instance).
        /// Please be aware that this introduces breaking changes as the mapping between tokens and their numerical
        /// representation (index) will change.
        /// </summary>
        /// <param name="minDocFrequency"></param>
        public void TruncateVocabulary(int minDocFrequency)
        {
            if (ReferenceCounts.TotalCounts != null && ReferenceCounts.TotalCounts.WordCounts.Count != 0)
                throw new Exception("did not except that ReferenceCounts.TotalCounts contains entries");
            var wordCounts = ReferenceCounts.DocCounts.WordCounts;
            var prevWordCount = wordCounts.Count;
            ReferenceCounts.DocCounts.RemoveEntriesBelowThreshold(minDocFrequency);
            wordCounts = ReferenceCounts.DocCounts.WordCounts;
            var numTermsRemoved = prevWordCount - wordCounts.Count;
           
            if (numTermsRemoved <= 0)
                return;

            var pairCounts = ReferenceCounts.DocCounts.PairCounts;
            //also clean word idx dict as it can also grow very large
            var newIdxMap = new WordIdxMap { TokenizationSettings = ReferenceIdxMap.TokenizationSettings };
            var idxToWordList = ReferenceIdxMap.IndexToWord;
            var oldIdxToNewIdxMap = new int[idxToWordList.Count];
            for (int i = 0; i < idxToWordList.Count; i++)
            {
                var token = idxToWordList[i];
                if (wordCounts.ContainsKey(i))
                {
                    var idx = newIdxMap.IndexToWord.Count;
                    newIdxMap.IndexToWord.Add(token);
                    newIdxMap.WordToIndex.Add(token, idx);
                    oldIdxToNewIdxMap[i] = idx;
                    continue;
                }

                //token with index i has been removed
                oldIdxToNewIdxMap[i] = -1;
            }

            ReferenceIdxMap = newIdxMap;
            var newWordCounts = new Dictionary<int, int>(wordCounts.Count);
            foreach (var p in wordCounts)
            {
                newWordCounts.Add(oldIdxToNewIdxMap[p.Key], p.Value);
            }
            ReferenceCounts.DocCounts.WordCounts = newWordCounts;
            if (pairCounts != null && pairCounts.Count != 0)
            {
                var newPairCounts = new Dictionary<WordIdxBigram, int>(pairCounts.Count);
                foreach (var p in pairCounts)
                {
                    newPairCounts.Add(new WordIdxBigram(oldIdxToNewIdxMap[p.Key.Idx1], oldIdxToNewIdxMap[p.Key.Idx2]),
                        p.Value);
                }
                ReferenceCounts.DocCounts.PairCounts = newPairCounts;
            }
        }

        /// <summary>
        /// Initialize keyphrase extractor with required reference statistics based on files in a folder (without subdirectories).
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <param name="settings">Parameters to influence tokenization and index building</param>
        /// <returns></returns>
        public static KeyphraseExtractor CreateFromFolder(string path,
            ElskeCreationSettings settings = null)
        {
            var fileNames = Directory.GetFiles(path);
            return CreateFromDocuments(fileNames.Select(File.ReadAllText), settings);
        }


        private double GetIdf(int idx)
        {
            return Math.Log(ReferenceCounts.DocCounts.NumDocuments /
                               (double)Math.Max(1, ReferenceCounts.DocCounts.WordCounts.GetValueOrDefault(idx, 1)));


        }



        private double GetIdf(WordIdxBigram pair)
        {
            return Math.Log(ReferenceCounts.DocCounts.NumDocuments /
                            (double)Math.Max(1, ReferenceCounts.DocCounts.PairCounts.GetValueOrDefault(pair, 1)));

        }


        private void EnsureStopWordsSet()
        {
            if (ReferenceIdxMap == null)
                throw new Exception($"{nameof(ReferenceIdxMap)} must be set");

            lock (ReferenceIdxMap)
            {
                if (_stopWordsSet != null)
                    return; //still up-to-date

                if (_stopWords == null || _stopWords.Length == 0)
                {
                    _stopWordsSet = new HashSet<int>(0);
                    return;
                }

                _stopWordsSet = new HashSet<int>(_stopWords.Length);
                foreach (var w in _stopWords)
                {
                    _stopWordsSet.Add(ReferenceIdxMap.GetIndex(w));
                }
            }
        }

        /// <summary>
        /// Transform document into TF-IDF weighted Bag-of-Words vector using ReferenceCounts for
        /// determining the inverse document frequency and StopWords for tokens to ignore
        /// </summary>
        /// <param name="document">document to convert</param>
        /// <param name="generateUnitVector">if true, vector will have unit length</param>
        /// <returns></returns>
        public (int key, float value)[] GenerateBoWVector(string document, bool generateUnitVector = false)
        {
            var tokenizedDoc = ReferenceIdxMap.DocumentToIndexes(document);
            return GenerateBoWVector(tokenizedDoc, generateUnitVector);
        }

        /// <summary>
        /// Transform document into TF-IDF weighted Bag-of-Words vector using ReferenceCounts for
        /// determining the inverse document frequency and StopWords for tokens to ignore
        /// </summary>
        /// <param name="tokenizedDoc">tokenized document to convert</param>
        /// <param name="generateUnitVector">if true, vector will have unit length</param>
        /// <returns></returns>
        public (int key, float value)[] GenerateBoWVector(IEnumerable<int> tokenizedDoc, bool generateUnitVector = false)
        {
            EnsureStopWordsSet();

            var dict = _dictLocal.Value;
            dict.Clear();

            foreach (var val in tokenizedDoc)
            {
                if (_stopWordsSet.Contains(val))
                    continue;
                dict.IncrementItem(val);
            }

            var res = new (int key, float value)[dict.Count];
            int j = 0;
            var valSum = 0d;
            foreach (var p in dict)
            {
                var val = p.Value * (float)ReferenceCounts.DocCounts.GetIdf(p.Key);
                res[j] = (p.Key, val);
                valSum += val * val;
                j++;
            }

            dict.Clear();

            if (!generateUnitVector || res.Length == 0 || valSum <= float.Epsilon)
                return res;

            var norm = (float)Math.Sqrt(valSum);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (res[i].key, res[i].value / norm);
            }

            return res;

        }


        /// <summary>
        /// Extract keyphrases from the given document. The document will be tokenized and
        /// converted into an integer-based representation using the reference index map.
        /// </summary>
        /// <param name="document">the document</param>
        /// <param name="numTopPhrases">maximum number of keyphrases to extract</param>
        /// <returns></returns>
        public List<PhraseResult> ExtractPhrases(string document, int numTopPhrases)
        {
            var tokenizedDocs = new[] { ReferenceIdxMap.DocumentToIndexes(document) };
            return ExtractPhrases(tokenizedDocs, numTopPhrases);
        }


        /// <summary>
        /// Extract keyphrases from the given collection of documents.
        /// Each document will be tokenized and converted into an integer-based representation
        /// using the reference index map.
        /// </summary>
        /// <param name="documents">collection of documents</param>
        /// <param name="numTopPhrases">maximum number of keyphrases to extract</param>
        /// <param name="processOnline">if true then the documents will be processed online, which uses less memory
        ///  but is also slower (less multi-threading, documents will be enumerated and converted twice)</param>
        /// <returns></returns>
        public List<PhraseResult> ExtractPhrases(IEnumerable<string> documents, int numTopPhrases, bool processOnline = false)
        {
            if (processOnline)
            {
                var query = documents.Select(doc =>
                {
                    var tokens = ReferenceIdxMap.TokenizeDocument(doc).ToArray();
                    return ReferenceIdxMap.TokensToIndexes(tokens);
                });

                return ExtractPhrases(query, numTopPhrases);
            }

            var arr = documents.Select(doc => ReferenceIdxMap.DocumentToIndexes(doc)).ToArray();
            return ExtractPhrases(arr, numTopPhrases);
        }

        /// <summary>
        /// Extract keyphrases from the given collection of tokenized documents.
        /// </summary>
        /// <param name="documents">collection of tokenized documents.
        /// It will be faster if this is a list-like structure.
        /// Will be enumerated twice.</param>
        /// <param name="numTopPhrases">maximum number of keyphrases to extract</param>
        /// <returns></returns>
        public List<PhraseResult> ExtractPhrases(IEnumerable<int[]> documents, int numTopPhrases)
        {
            EnsureStopWordsSet();

            var watch = Stopwatch.StartNew();

            if (ReferenceCounts.DocCounts.NumDocuments < 1
                || ReferenceCounts.DocCounts.PairCounts == null || ReferenceCounts.DocCounts.PairCounts.Count == 0
                || ReferenceCounts.DocCounts.WordCounts == null || ReferenceCounts.DocCounts.WordCounts.Count == 0)
                throw new Exception("ReferenceCounts have to contain word and pair counts of a reference collection");

            if(CountRequirements?.MinPhraseDocCount > 0 && ReferenceDocuments == null)
                throw new Exception("ReferenceDocuments must be set (BuildReferenceCollection = true) for MinPhraseFrequencyInReferenceCollection > 0");


            var maxIdf = Math.Log(ReferenceCounts.DocCounts.NumDocuments);
            var localCounts = CorpusCounts.GetTotalCountsOnly(documents);
            if (localCounts.TotalCounts.NumDocuments == 0)
                return new List<PhraseResult>();
            var rootExp = 1d;
            var powExp = 1d;

            var useRootExp = false;
            if (UsePfIdf)
            {
                //max "term frequency" component should be in the ballpark of idf values
                //->  maxTf^(1/powExp) = 500 -> powExp = log_500(maxTf)

                var maxTf = localCounts.TotalCounts.WordCounts.Values.Max();

                if (maxTf > 500)
                {
                    powExp = Math.Log(maxTf, 500);
                    rootExp = 1 / powExp;
                    useRootExp = true;

                    if (IsDebugTextOutputEnabled)
                        Trace.WriteLine($"max tf {maxTf} maxidf {maxIdf} | pow exp: {powExp} |root exp: {rootExp}");
                }
            }

            double TransformTf(int tf)
            {
                return useRootExp ? Math.Pow(tf, rootExp) : tf;
            }

            var tooShortWords = new HashSet<int>();

            if (MinNumCharacters > 1)
            {
                foreach (var idx in localCounts.TotalCounts.WordCounts.Keys)
                {
                    if (ReferenceIdxMap.GetTokenAsMemory(idx).Length < MinNumCharacters)
                        tooShortWords.Add(idx);
                }
            }

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for counting local set");
                watch.Restart();
            }

            var wordTfIdfs = new List<(int, double)>(localCounts.TotalCounts.WordCounts.Count);
            var pairTfIdfs = new List<(WordIdxBigram, double)>(Mode > ExtractingMode.UnigramsOnly
                ? localCounts.TotalCounts.PairCounts.Count
                : 0);

            var initialTfIdfList = new List<double>(wordTfIdfs.Count + pairTfIdfs.Count);

            Dictionary<int, int> wordToHighestBigramCount = null;

            if (Mode > ExtractingMode.UnigramsOnly)
            {
                //for a better determination of the min term/phrase frequency in the case of extracting
                //longer phrases, we do not want to consider words that mainly appear together in a phrase
                //because these words "collapse" in the later stage into one phrase, reducing the resulting num phrases
                //we therefore first retrieve maximum of how often a specific term appears with another term
                wordToHighestBigramCount = new Dictionary<int, int>(localCounts.TotalCounts.WordCounts.Count);

                foreach (var p in localCounts.TotalCounts.PairCounts)
                {
                    if (_stopWordsSet.Contains(p.Key.Idx1) || _stopWordsSet.Contains(p.Key.Idx2))
                        continue;

                    wordToHighestBigramCount.SetItemToMax(p.Key.Idx1, p.Value);
                    wordToHighestBigramCount.SetItemToMax(p.Key.Idx2, p.Value);
                }
            }

            foreach (var p in localCounts.TotalCounts.WordCounts)
            {
                if (_stopWordsSet.Contains(p.Key) ||
                    MinNumCharacters > 1 && tooShortWords.Contains(p.Key))
                    continue;

                var tfidf = TransformTf(p.Value) * GetIdf(p.Key);

                wordTfIdfs.Add((p.Key, tfidf));


                if (wordToHighestBigramCount != null &&
                    wordToHighestBigramCount.TryGetValue(p.Key, out var bigramCount))
                {
                    if (bigramCount >= p.Value / 2)
                    {
                        //term appears in majority of cases with another term
                        //do not use count for determining threshold
                        continue;
                    }
                }


                initialTfIdfList.Add(tfidf);
            }

            if (Mode > ExtractingMode.UnigramsOnly)
            {

                //first calculate threshold regarding unigrams only so we can skip certain infrequent bigrams already
                //this can lead to speedup of 10x or more for this block
                initialTfIdfList.Sort();


                var uniTfidfTh =
                    initialTfIdfList.Count < numTopPhrases ? 0 : initialTfIdfList[^numTopPhrases];

                var uniMinTransTf = uniTfidfTh / maxIdf;
                var uniMinTfTh = Math.Max(1, (int)(useRootExp ? Math.Pow(uniMinTransTf, powExp) : uniMinTransTf));



                foreach (var p in localCounts.TotalCounts.PairCounts)
                {
                    if (p.Value < uniMinTfTh)
                        continue;


                    var numStopwords = _stopWordsSet.Contains(p.Key.Idx1) ? 1 : 0;
                    if (_stopWordsSet.Contains(p.Key.Idx2))
                        numStopwords++;

                    if (numStopwords == 2)
                        continue;

                    if (numStopwords == 1 && p.Value <= 2)
                        continue;

                    if (MinNumCharacters > 1 &&
                       tooShortWords.Contains(p.Key.Idx1) &&
                       tooShortWords.Contains(p.Key.Idx2))
                        continue; //at least one word has to meet min no chars requirement

                    var tfIdf = TransformTf(p.Value) * GetIdf(p.Key);
                    if (tfIdf < uniTfidfTh)
                        continue;

                    pairTfIdfs.Add((p.Key, tfIdf));

                    if (numStopwords >= 1)
                        continue;

                    if ((localCounts.TotalCounts.WordCounts[p.Key.Idx1] <= 2 * p.Value
                        || localCounts.TotalCounts.WordCounts[p.Key.Idx2] <= 2 * p.Value))
                    {
                        //we ignore this tfidf value for determining the threshold as it could be merged at a later stage
                        continue;
                    }

                    initialTfIdfList.Add(tfIdf);

                }

            }

            var beforeSortElapsed = watch.Elapsed;


            initialTfIdfList.Sort();

            const double defaultMinTfIdfTh = 3;


            var tfidfTh = Math.Max(defaultMinTfIdfTh,
                initialTfIdfList.Count < numTopPhrases ? 0 : initialTfIdfList[^numTopPhrases]);

            var minTransTf = tfidfTh / maxIdf;
            var minTfTh = Math.Max(1, (int)(useRootExp ? Math.Pow(minTransTf, powExp) : minTransTf));

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for tf thresholds, sorting: {watch.Elapsed - beforeSortElapsed}");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"tfidf th {tfidfTh} | max idf {maxIdf} | min tf {minTfTh}");


            //extract candidate patterns with more than two words

            Dictionary<WordSequence, int> phraseCandidates = new Dictionary<WordSequence, int>();

            var phraseMinTfTh = Math.Max(CountRequirements?.MinPhraseFrequency ?? 1, minTfTh);
            if (Mode >= ExtractingMode.Phrases)
            {
                phraseCandidates = GetPhraseCandidates(documents, localCounts, phraseMinTfTh);
                //phraseCandidates = GetPhraseCandidates2(sentences, localCounts, minTfTh);
            }

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for phrase candidates");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"{phraseCandidates.Count} phrase candidates extracted");

            foreach (var k in phraseCandidates.Where(c => c.Value < phraseMinTfTh)
                .Select(c => c.Key).ToArray())
            {
                phraseCandidates.Remove(k);
            }


            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for cleaning phrase candidates below th");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"{phraseCandidates.Count} phrase candidates extracted after th cleaning");



            if (Mode == ExtractingMode.DebugOnlyPhraseCandidates)
            {
                return phraseCandidates.Select(p =>
                    new PhraseResult(p.Key, p.Value, 0, ReferenceIdxMap.WordSequenceToString(p.Key))).ToList();
            }

            var phraseCandidatesList = phraseCandidates.ToArray();
            for (int i = 1; i < phraseCandidatesList.Length; i++)
            {
                ref var prev = ref phraseCandidatesList[i - 1];
                ref var cur = ref phraseCandidatesList[i];

                if (prev.Value > 1 && prev.Value == cur.Value &&
                    prev.Key.Matches(cur.Key))
                    phraseCandidates.Remove(phraseCandidatesList[i - 1].Key);
            }


            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for quick cleaning phrase candidates with obvious substrings");
                watch.Restart();
            }


            if (Mode >= ExtractingMode.Phrases && MinNumCharacters > 1)
            {
                var list = phraseCandidates.Keys.ToArray();
                for (int i = 0; i < list.Length; i++)
                {
                    var item = list[i].Indexes;

                    var delete = true;
                    for (int j = 0; j < item.Length; j++)
                    {
                        var val = item[j];
                        if (tooShortWords.Contains(val)) continue;

                        delete = false;
                        break;
                    }

                    if (delete)
                        phraseCandidates.Remove(list[i]);

                }
            }


            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for cleaning candidates that only consist of short words");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"{phraseCandidates.Count} phrase candidates extracted after quick cleaning");



            RemoveShorterPhrasesWithEqualFrequency(phraseCandidates);

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for extensive cleaning phrase candidates with substrings");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
            {
                Trace.WriteLine($"{phraseCandidates.Count} phrase candidates extracted after extensive cleaning");
                Trace.WriteLine("");
                foreach (var p in phraseCandidates)
                {
                    Trace.WriteLine($"{ReferenceIdxMap.WordSequenceToString(p.Key)} | {p.Value}");
                }
                Trace.WriteLine("\r\n");
            }

            //get idf for patterns
            var phraseCandidateWordSequencesDocCount =
                Mode >= ExtractingMode.Phrases ? GetDocCountOfPhrases(phraseCandidates) : null;

            var patternsTfIdf = new Dictionary<WordSequence, double>();
            var patternsTf = new Dictionary<WordSequence, int>();
            foreach (var t in wordTfIdfs)
            {
                if (t.Item2 < tfidfTh)
                    continue;


                patternsTfIdf.Add(new WordSequence(new[] { t.Item1 }), t.Item2);
                patternsTf.Add(new WordSequence(new[] { t.Item1 }), localCounts.TotalCounts.WordCounts[t.Item1]);
            }

            if (Mode >= ExtractingMode.UniAndBigramsOnly)
            {

                foreach (var t in pairTfIdfs)
                {
                    if (t.Item2 < tfidfTh)
                        continue;


                    patternsTf.Add(new WordSequence(new[] { t.Item1.Idx1, t.Item1.Idx2 }),
                        localCounts.TotalCounts.PairCounts[new WordIdxBigram(t.Item1.Idx1, t.Item1.Idx2)]);

                    patternsTfIdf.Add(new WordSequence(new[] { t.Item1.Idx1, t.Item1.Idx2 }), t.Item2);
                }
            }


            var noLongerWordSequencesAdded = 0;

            if (phraseCandidateWordSequencesDocCount != null)
            {

                foreach (var p in phraseCandidateWordSequencesDocCount)
                {
                    var idf = Math.Log(ReferenceCounts.DocCounts.NumDocuments / (double)Math.Max(1, p.Value));
                    var tf = phraseCandidates[p.Key];

                    var tfidf = TransformTf(tf) * idf;
                    if (tfidf < tfidfTh)
                        continue;

                    noLongerWordSequencesAdded++;

                    patternsTfIdf.Add(p.Key, tfidf);
                    patternsTf.Add(p.Key, tf);
                    //patternsIdf.Add(p.Key, idf);
                }
            }

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for phrase candidates idf retrieval");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"{noLongerWordSequencesAdded} final phrase candidates added\r\n");


            //hierarchy contains for every pattern all other patterns that contain that pattern
            var hierarchy = GetWordSequenceHierarchy(patternsTfIdf.Keys, patternsTf);

            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for pattern hierarchy");
                watch.Restart();
            }

            if (IsDebugTextOutputEnabled && false)
            {

                Trace.WriteLine("\r\nHIERARCHY:\r\n");
                foreach (var branch in hierarchy)
                {
                    foreach (var pattern in branch)
                    {
                        Trace.WriteLine($"{ReferenceIdxMap.WordSequenceToString(pattern)}\t [{patternsTf[pattern]}]");
                    }
                    Trace.WriteLine("");
                }
            }

            //remove terms and bigrams with equal frequency to a longer phrase,
            var toRemoveSet = new HashSet<WordSequence>();
            foreach (var branch in hierarchy)
            {
                if (branch.Count <= 1)
                    continue;

                var baseItem = branch[0];
                if (baseItem.Indexes.Length > 2)
                    continue; //we only have to deal with uni and bigrams here, longer patterns are not redundant with equal phrase frequency here anymore

                var tf = patternsTf[baseItem];
                if (tf <= 1)
                    continue; //words and bigrams with only one occurrence are special

                var numStopWordsBaseItem = GetNumStopWords(baseItem.Indexes);
                var numNonStopWordsBaseItem = baseItem.Indexes.Length - numStopWordsBaseItem;



                for (int i = 1; i < branch.Count; i++)
                {
                    var longerItem = branch[i];
                    if (patternsTf[longerItem] == tf)
                    {
                        var numStopWords = GetNumStopWords(longerItem.Indexes);
                        if (longerItem.Indexes.Length - numStopWords > numNonStopWordsBaseItem)
                        {
                            //we have longer phrase with equal frequency that has at least one additional term
                            //which is not a stop word
                            toRemoveSet.Add(baseItem);
                            break;
                        }
                    }
                }
            }

            if (IsDebugTextOutputEnabled)
            {
                Trace.WriteLine("\r\nremoved due to equal frequency:");
                foreach (var wordSequence in toRemoveSet)
                {
                    Trace.WriteLine(ReferenceIdxMap.WordSequenceToString(wordSequence));
                }
            }

            if (Mode >= ExtractingMode.PhrasesRemoveLongerVariations)
            {
                //we now remove longer variations in which "overhanging part" is not information worthy enough

                //helper function to quickly calculate tf-idf/pf-idf of a term or pair
                double GetTermOrPairTfIdf(int word1, int word2)
                {
                    var docFreq = word2 < 0
                        ? ReferenceCounts.DocCounts.WordCounts.GetValueOrDefault(word1)
                        : ReferenceCounts.DocCounts.PairCounts.GetValueOrDefault(new WordIdxBigram(word1,
                            word2));

                    var tf = word2 < 0
                        ? localCounts.TotalCounts.WordCounts.GetValueOrDefault(word1)
                        : localCounts.TotalCounts.PairCounts.GetValueOrDefault(new WordIdxBigram(
                            word1,
                            word2));

                    var idf = Math.Log(ReferenceCounts.DocCounts.NumDocuments / (double)Math.Max(1, docFreq));

                    return TransformTf(tf) * idf;
                }

                if (Mode >= ExtractingMode.Full)
                {
                    foreach (var branch in hierarchy)
                    {
                        var baseItem = branch[0];

                        if (baseItem.Indexes.Length > 4)
                            break;

                        if (toRemoveSet.Contains(baseItem))
                            continue;

                        if (baseItem.Indexes.Length <= 1) continue;

                        //for the Full mode, we look at special cases with 2,3 or 4 terms, but which
                        //contain only one term that is not a stop word (e.g., "at the sea")
                        //in this case we remove the phrase if the non-stop word is not above our threshold
                        //(if it is above the threshold, the subsequent cleaning process will kick in)
                        var numStopWords = 0;
                        var notStopWordIdx = -1;
                        for (int i = 0; i < baseItem.Indexes.Length; i++)
                        {
                            if (_stopWordsSet.Contains(baseItem.Indexes[i]))
                                numStopWords++;
                            else
                                notStopWordIdx = baseItem.Indexes[i];
                        }

                        if (baseItem.Indexes.Length - numStopWords == 1 &&
                            GetTermOrPairTfIdf(notStopWordIdx, -1) < tfidfTh)
                        {
                            toRemoveSet.Add(baseItem);
                        }
                    }
                }


                foreach (var branch in hierarchy)
                {
                    var baseItem = branch[0];

                    if (branch.Count <= 1)
                        continue;

                    for (int i = 1; i < branch.Count; i++)
                    {
                        var longerItem = branch[i];

                        //we now look at children that have no more than two additional words 
                        //to the left and no more than two to the right

                        if (longerItem.Indexes.Length - baseItem.Indexes.Length > 4)
                        {
                            break;
                        }

                        if (toRemoveSet.Contains(longerItem))
                            continue;

                        //determine index where parent = baseItem starts
                        var startIdx = WordSequence.FindIndex(longerItem.Indexes,
                            baseItem.Indexes);

                        var afterIdx = startIdx + baseItem.Indexes.Length;
                        var remainingNumWords = longerItem.Indexes.Length - afterIdx;

                        //Trace.WriteLine($"checking: {baseItem.ToString(ReferenceIdxMap.IdxToWord)} | longer item {longerItem.ToString(ReferenceIdxMap.IdxToWord)} | start ix {startIdx} remaining num words {remainingNumWords}");
                        if (startIdx > 2 || remainingNumWords > 2)
                        {
                            continue;
                        }

                        //left overhang

                        var leftIsNotNew = true;
                        if (startIdx == 1 && !_stopWordsSet.Contains(longerItem.Indexes[0]) ||
                            (startIdx == 2 &&
                             (!_stopWordsSet.Contains(longerItem.Indexes[0]) || !_stopWordsSet.Contains(longerItem.Indexes[1]))))
                        {
                            //new words left to original word are not all stop words

                            var tfidf = GetTermOrPairTfIdf(longerItem.Indexes[0],
                                startIdx <= 1 ? -1 : longerItem.Indexes[1]);

                            //we want to remove longer items of stop words in any case,
                            //but if the baseItem was removed (maybe due to equal frequency), we still want
                            //to include the combined term

                            if (tfidf >= tfidfTh * OverhangTfIdfThresholdFactor)
                                leftIsNotNew = false;

                        }

                        if (!leftIsNotNew)
                        {
                            continue;
                        }


                        var rightIsNotNew = true;


                        if (remainingNumWords == 1 && !_stopWordsSet.Contains(longerItem.Indexes[afterIdx]) ||
                            (remainingNumWords == 2 &&
                             (!_stopWordsSet.Contains(longerItem.Indexes[afterIdx]) || !_stopWordsSet.Contains(longerItem.Indexes[afterIdx + 1]))))
                        {
                            //new words right to original word are not (all) stop words

                            var tfidf = GetTermOrPairTfIdf(longerItem.Indexes[afterIdx],
                                remainingNumWords <= 1 ? -1 : longerItem.Indexes[afterIdx + 1]);

                            if (tfidf >= tfidfTh * OverhangTfIdfThresholdFactor)
                                rightIsNotNew = false;
                        }


                        if (rightIsNotNew)
                        {
                            toRemoveSet.Add(longerItem);
                        }


                    }
                }


                if (Mode >= ExtractingMode.Full)
                {

                    //now remove parents = smaller phrases if children already make up for most occurrences
                    var hierarchyDict = hierarchy.ToDictionary(l => l[0], l => l);

                    var patternsToIgnore = new HashSet<WordSequence>();
                    var currentPatterns = new List<WordSequence>();

                    foreach (var branch in hierarchy)
                    {
                        if (branch.Count <= 1)
                            continue;

                        var baseItem = branch[0];

                        if (toRemoveSet.Contains(baseItem))
                            continue;

                        patternsToIgnore.Clear();
                        currentPatterns.Clear();

                        var tfToSubtract = 0;

                        for (int i = 1; i < branch.Count; i++)
                        {
                            var longerItem = branch[i];


                            if (toRemoveSet.Contains(longerItem) || patternsToIgnore.Contains(longerItem))
                                continue;


                            var children = hierarchyDict[longerItem];
                            if (patternsToIgnore.Contains(children.Last()))
                                continue;

                            if (currentPatterns.Count > 0)
                            {
                                var idxOfBaseInLongerItem = longerItem.Indexes.AsSpan().IndexOf(baseItem.Indexes);

                                var isIncompatible = false;
                                for (int j = 0; j < currentPatterns.Count; j++)
                                {
                                    var curItem = currentPatterns[j].Indexes;
                                    var idxOfBaseInCurItem =
                                        curItem.AsSpan().IndexOf(baseItem.Indexes);

                                    for (int aIdx = idxOfBaseInLongerItem - 1, bIdx = idxOfBaseInCurItem - 1;
                                        aIdx >= 0 && bIdx >= 0; aIdx--, bIdx--)
                                    {
                                        if (longerItem.Indexes[aIdx] == curItem[bIdx]) continue;
                                        isIncompatible = true;
                                        break;
                                    }

                                    if (isIncompatible)
                                        break;

                                    for (int aIdx = idxOfBaseInLongerItem + baseItem.Indexes.Length,
                                        bIdx = idxOfBaseInCurItem + baseItem.Indexes.Length;
                                        aIdx < longerItem.Indexes.Length && bIdx < curItem.Length;
                                        aIdx++, bIdx++)
                                    {
                                        if (longerItem.Indexes[aIdx] == curItem[bIdx]) continue;

                                        isIncompatible = true;
                                        break;
                                    }

                                    if (isIncompatible)
                                        break;


                                }

                                if (!isIncompatible)
                                    continue;
                            }

                            tfToSubtract += patternsTf[longerItem];
                            currentPatterns.Add(longerItem);


                            foreach (var wordSequence in children)
                            {
                                //this term frequency already "contains" children of this longerItem
                                patternsToIgnore.Add(wordSequence);
                            }

                        }

                        if (tfToSubtract <= 0)
                            continue;

                        var basePatternIdf = patternsTfIdf[baseItem] / TransformTf(patternsTf[baseItem]);
                        var basePatternNewTf = patternsTf[baseItem] - tfToSubtract;

                        if (basePatternNewTf <= 0 ||
                           TransformTf(basePatternNewTf) * basePatternIdf < tfidfTh)
                            toRemoveSet.Add(baseItem);
                    }
                }
            }


            if (IsDebugStopwatchEnabled)
            {
                Trace.WriteLine($"{watch.Elapsed} for removing of variations");
                watch.Restart();
            }


            if (IsDebugTextOutputEnabled)
                Trace.WriteLine($"\r\n{toRemoveSet.Count} patterns to remove due to only slight variations:");

            foreach (var pattern in toRemoveSet)
            {

                if (IsDebugTextOutputEnabled)
                    Trace.WriteLine(ReferenceIdxMap.WordSequenceToString(pattern));
                patternsTfIdf.Remove(pattern);
            }


            if (IsDebugTextOutputEnabled)
            {
                Trace.WriteLine("\r\n");


                foreach (var p in patternsTfIdf
                    .OrderByDescending(p => p.Value))
                {
                    Trace.WriteLine($"{ReferenceIdxMap.WordSequenceToString(p.Key)} | tfidf {p.Value:N2} | count {patternsTf[p.Key]}");
                }
            }

            var hasCountReq = CountRequirements != null && (
                CountRequirements.MinUnigramFrequency > 0 ||
                CountRequirements.MinUnigramDocCount > 0 ||
                CountRequirements.MinBigramFrequency > 0 ||
                CountRequirements.MinBigramDocCount > 0 ||
                CountRequirements.MinPhraseFrequency > 0 ||
                CountRequirements.MinPhraseDocCount > 0);

            return patternsTfIdf
                .Where(p =>
                {
                    if (p.Value < tfidfTh)
                        return false;
                    if (!hasCountReq)
                        return true;
                    var tf = patternsTf[p.Key];
                    switch (p.Key.Indexes.Length)
                    {
                        case 1:
                            if (CountRequirements.MinUnigramFrequency > 0 &&
                                tf < CountRequirements.MinUnigramFrequency)
                                return false;
                            if (CountRequirements.MinUnigramDocCount > 0 &&
                                ReferenceCounts.DocCounts.WordCounts.GetValueOrDefault(p.Key.Indexes[0]) <
                                CountRequirements.MinUnigramDocCount)
                                return false;
                            return true;
                        case 2:
                            if (CountRequirements.MinBigramFrequency > 0 &&
                                tf < CountRequirements.MinBigramFrequency)
                                return false;
                            if (CountRequirements.MinBigramDocCount > 0 &&
                                ReferenceCounts.DocCounts.PairCounts.GetValueOrDefault(new WordIdxBigram(p.Key.Indexes[0], p.Key.Indexes[1])) <
                                CountRequirements.MinBigramDocCount)
                                return false;
                            return true;
                        case > 2:
                            if (CountRequirements.MinPhraseFrequency > 0 &&
                                tf < CountRequirements.MinPhraseFrequency)
                                return false;
                            if (CountRequirements.MinPhraseDocCount > 0 &&
                                ReferenceDocuments.GetDocumentFrequency(p.Key.Indexes) < CountRequirements.MinPhraseDocCount)
                                return false;
                            return true;
                        default:
                            return false;
                    }
                    
                    return p.Value >= tfidfTh;
                })
                .OrderByDescending(p => p.Value)
                .Take(numTopPhrases)
                .Select(p => new PhraseResult(p.Key, patternsTf[p.Key], p.Value,
                    ReferenceIdxMap.WordSequenceToString(p.Key))).ToList();


        }

        private int GetNumStopWords(int[] indexes)
        {
            var numStopWords = 0;
            foreach (var index in indexes)
            {
                if (_stopWordsSet.Contains(index))
                    numStopWords++;
            }

            return numStopWords;
        }


        private Dictionary<WordSequence, int> GetDocCountOfPhrases(Dictionary<WordSequence, int> phraseCandidates)
        {
            var phraseCandidateWordSequencesDocCount =
                phraseCandidates.ToDictionary(k => k.Key, v => 0);


            if (ReferenceDocuments != null)
            {
                var keys = phraseCandidates.Keys.ToArray();
#if SINGLE_CORE_ONLY
                for (var i = 0; i < keys.Length; i++)
                {
                    var p = keys[i];
                    var arr = p.Indexes;
                    var docCount = MainIndex.GetDocumentFrequency(arr);
                    phraseCandidateWordSequencesDocCount[p] = docCount;

                }
#else
                Parallel.For(0, keys.Length, i =>
                {
                    var p = keys[i];
                    var arr = p.Indexes;
                    var docCount = ReferenceDocuments.GetDocumentFrequency(arr);

                    lock (phraseCandidateWordSequencesDocCount)
                        phraseCandidateWordSequencesDocCount[p] = docCount;
                });
#endif

                return phraseCandidateWordSequencesDocCount;

            }


            var pairToPhraseCandidateWordSequences = new Dictionary<WordIdxBigram, List<WordSequence>>();

            foreach (var p in phraseCandidates.Keys)
            {
                var arr = p.Indexes;
                WordIdxBigram bestPair = default;
                var frequency = int.MaxValue;
                for (int i = 1; i < arr.Length; i++)
                {
                    var pair = new WordIdxBigram(arr[i - 1], arr[i]);
                    var tf = ReferenceCounts.DocCounts.PairCounts.GetValueOrDefault(pair);
                    if (tf < frequency)
                    {
                        frequency = tf;
                        bestPair = pair;
                    }
                }

                pairToPhraseCandidateWordSequences.AddToList(bestPair, p);

                //fallback because no full reference collection available: we just use rarest bigram frequency as estimate

                phraseCandidateWordSequencesDocCount[p] = Math.Max(1, frequency);


            }

            return phraseCandidateWordSequencesDocCount;

        }

        public static void RemoveShorterPhrasesWithEqualFrequency(Dictionary<WordSequence, int> phraseCandidates)
        {
            var pairToPhraseCandidateWordSequences = new Dictionary<WordIdxBigram, List<WordSequence>>();
            foreach (var p in phraseCandidates.Keys)
            {
                var arr = p.Indexes;
                for (int i = 1; i < arr.Length; i++)
                {
                    var pair = new WordIdxBigram(arr[i - 1], arr[i]);
                    pairToPhraseCandidateWordSequences.AddToList(pair, p);
                }
            }

            var toRemove = new List<WordSequence>();

            foreach (var p in phraseCandidates)
            {
                var arr = p.Key.Indexes;
                List<WordSequence> patternsToCheck = null;

                for (int i = 1; i < arr.Length; i++)
                {
                    var pair = new WordIdxBigram(arr[i - 1], arr[i]);
                    var possibleWordSequences = pairToPhraseCandidateWordSequences.GetValueOrDefault(pair);
                    if (possibleWordSequences == null)
                        break; //shouldn't happen

                    if (patternsToCheck == null || possibleWordSequences.Count < patternsToCheck.Count)
                        patternsToCheck = possibleWordSequences;
                }

                if (patternsToCheck == null)
                    continue;

                foreach (var pattern in patternsToCheck)
                {
                    if (pattern.Indexes.Length <= arr.Length)
                        continue;

                    if (phraseCandidates[pattern] != p.Value)
                        continue;

                    if (!p.Key.Matches(pattern))
                        continue;

                    toRemove.Add(p.Key);
                    break;
                }
            }

            foreach (var p in toRemove)
            {
                phraseCandidates.Remove(p);
            }

        }


        private Dictionary<WordSequence, int> GetPhraseCandidates(IEnumerable<int[]> sentences, CorpusCounts localCounts,
            int minTfTh)
        {

            var patternRecycling = new WordSequenceRecycler();

            void ProcessDocument(Dictionary<WordSequence, int> phraseCandidates, List<int> patternTemp, int[] arr)
            {
                var lim = arr.Length - 2;
                var pairCounts = localCounts.TotalCounts.PairCounts;
                for (int i = 0; i < lim; i++)
                {
                    patternTemp.Clear();
                    var patternHashCode = Extensions.Fnv1StartHash32;
                    var onlyStopWords = true;
                    var numNonStopWords = 0;

                    var val = arr[i];
                    patternTemp.Add(val);
                    patternHashCode = (unchecked((uint)val)).ToFnv1_32(patternHashCode);

                    if (!_stopWordsSet.Contains(val))
                    {
                        onlyStopWords = false;
                        numNonStopWords++;
                    }

                    for (int j = i + 1; j < arr.Length; j++)
                    {
                        var maxTf = pairCounts.GetValueOrDefault(
                            new WordIdxBigram(arr[j - 1], arr[j]), 1);

                        if (maxTf < minTfTh || patternTemp.Count >= MaxNumWords)
                        {
                            //bigram at pos j-1 is too rare or max length reached
                            break;
                        }

                        if (maxTf < 3 && numNonStopWords != patternTemp.Count)
                            break; //

                        val = arr[j];
                        patternTemp.Add(val);
                        patternHashCode = unchecked ((uint)val).ToFnv1_32(patternHashCode);

                        if (!_stopWordsSet.Contains(val))
                        {
                            onlyStopWords = false;
                            numNonStopWords++;
                        }

                        if (onlyStopWords || patternTemp.Count < 3)
                            continue;

                        var pattern = patternRecycling.RetrieveOrCreate(unchecked((int)patternHashCode), patternTemp);
                        phraseCandidates.IncrementItem(pattern);
                    }
                }
            }


#if SINGLE_CORE_ONLY

            var patternTemp = new List<int>();
            var phraseCandidates = new Dictionary<WordSequence, int>();

            foreach (var arr in sentences)
            {
                ProcessDocument(phraseCandidates, patternTemp, arr);
            }

            return phraseCandidates;
#else

            using var phraseCandidatesLocal = new ThreadLocal<Dictionary<WordSequence, int>>(() => new Dictionary<WordSequence, int>(), true);
            using var patternTempLocal = new ThreadLocal<List<int>>(() => new List<int>());

            if (sentences is IList<int[]> sentencesList)
            {
                //we can use range-based multi-threading if we have list-like structure
                var rangePartitioner = Partitioner.Create(0, sentencesList.Count);
                Parallel.ForEach(rangePartitioner, (range, state) =>
                {
                    var patternTemp = patternTempLocal.Value;
                    var phraseCandidates = phraseCandidatesLocal.Value;
                    for (int k = range.Item1; k < range.Item2; k++)
                    {
                        var arr = sentencesList[k];
                        ProcessDocument(phraseCandidates, patternTemp, arr);
                    }
                });
            }
            else
            {
                var patternTemp = patternTempLocal.Value;
                var phraseCandidates = phraseCandidatesLocal.Value;
                foreach (var arr in sentences)
                {
                    ProcessDocument(phraseCandidates, patternTemp, arr);
                }
            }

            var res = CorpusCounts.MergeThreadLocalDictionaries(phraseCandidatesLocal);

            return res;

#endif

        }



        /// <summary>
        /// Given word sequences (patterns), we want to establish parent->child relationship in terms
        /// of subsequences, i.e., children are all the patterns that match the parent
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="patternsTf"></param>
        /// <returns>For each input pattern, sorted list of children (increasing sequence length)</returns>

        private static List<List<WordSequence>> GetWordSequenceHierarchy(IEnumerable<WordSequence> patterns,
            Dictionary<WordSequence, int> patternsTf)
        {
            //'ordered' contains patterns in increasing sequence length
            var ordered = patterns
                .OrderBy(p => p.Indexes.Length)
                .ThenByDescending(p => patternsTf[p])
                .ToArray();
            //create term and bigram index of patterns to make subsequent algorithm quicker
            //i.e., wordIdxPair -> list of sequence that contain word (word, -1) or bigram (word1, word2)
            var dict = new Dictionary<WordIdxBigram, List<WordSequence>>();
            foreach (var p in ordered)
            {
                var arr = p.Indexes;
                if (arr.Length <= 1)
                    continue;//single term can never be child of any other phrase

                dict.AddToList(new WordIdxBigram(arr[0], -1), p);

                for (int i = 1; i < arr.Length; i++)
                {
                    dict.AddToList(new WordIdxBigram(arr[i], -1), p);
                    var pair = new WordIdxBigram(arr[i - 1], arr[i]);
                    dict.AddToList(pair, p);
                }
            }
            //make index distinct
            var set = new HashSet<WordSequence>();
            foreach (var list in dict.Values)
            {
                if (list.Count <= 1)
                    continue;
                set.Clear();
                var hasDuplicates = false;
                foreach (var it in list)
                {
                    if (!set.Add(it))
                        hasDuplicates = true;
                }
                if (!hasDuplicates)
                    continue;
                list.Clear();
                foreach (var it in set)
                {
                    list.Add(it);
                }
            }

            var res = new List<List<WordSequence>>();

            for (var m = 0; m < ordered.Length; m++)
            {
                ref var p = ref ordered[m];
                //'branch' is now list for parent = item in 'res'
                var branch = new List<WordSequence>(1) { p };
                res.Add(branch);

                if (p.Indexes.Length <= 2)
                {
                    //special case for terms and bigrams
                    //because we already built index, we can use that index to determine all children
                    var pair = new WordIdxBigram(p.Indexes[0],
                            p.Indexes.Length <= 1 ? -1 : p.Indexes[1]);


                    if (dict.TryGetValue(pair, out var list))
                    {
                        foreach (var pattern in list)
                        {
                            if (pattern.Indexes.Length == p.Indexes.Length)
                                continue; //same word or bigram
                            branch.Add(pattern);
                        }
                    }

                    continue;
                }

                //to get children, we first get smallest list of candidates from index
                //we know that child has to contain every bigram of parent, i.e., we can
                //use rarest bigram for list of candidates
                List<WordSequence> patternsToCheck = null;
                var arr = p.Indexes;
                for (int i = 1; i < arr.Length; i++)
                {
                    var pair = new WordIdxBigram(arr[i - 1], arr[i]);
                    var possibleWordSequences = dict.GetValueOrDefault(pair);
                    if (possibleWordSequences == null)
                        break; //shouldn't happen

                    if (patternsToCheck == null || possibleWordSequences.Count < patternsToCheck.Count)
                        patternsToCheck = possibleWordSequences;
                }

                if (patternsToCheck == null)
                    continue;

                //check candidates whether they contain parent
                foreach (var pattern in patternsToCheck)
                {
                    if (pattern.Indexes.Length <= arr.Length)
                        continue;


                    if (!p.Matches(pattern))
                        continue;

                    branch.Add(pattern);
                }
            }

            for (int i = 0; i < res.Count; i++)
            {
                res[i] = res[i].OrderBy(it => it.Indexes.Length)
                    .ThenByDescending(it => patternsTf[it]).ToList();
            }

            return res;
        }
    }


}
