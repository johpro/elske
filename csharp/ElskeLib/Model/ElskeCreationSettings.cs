/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace ElskeLib.Model
{
    public class ElskeCreationSettings
    {
        /// <summary>
        /// Parameters on how to tokenize documents, e.g., case sensitivity
        /// </summary>
        public TokenizationSettings TokenizationSettings { get; set; } = new TokenizationSettings();
        /// <summary>
        /// If true, the complete collection will be indexed and saved to retrieve
        /// exact document frequencies of phrases
        /// </summary>
        public bool BuildReferenceCollection { get; set; }
        /// <summary>
        /// If true, bigrams will not be counted to save time in case KeyphraseExtractor is only used to
        /// create Bag-of-Words representations or to access the reference map.
        /// </summary>
        public bool DoNotCountPairs { get; set; }


    }
}
