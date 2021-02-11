/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace ElskeLib.Model
{
    public readonly struct PhraseResult
    {
        public PhraseResult(WordSequence pattern, int termFrequency, double tfIdf, string phrase)
        {
            WordSequence = pattern;
            TermFrequency = termFrequency;
            TfIdf = tfIdf;
            Phrase = phrase;
        }

        public string Phrase { get; }

        public WordSequence WordSequence { get; }
        public int TermFrequency { get; }
        public double TfIdf { get; }

        public override string ToString()
        {
            return Phrase;
        }
    }
}