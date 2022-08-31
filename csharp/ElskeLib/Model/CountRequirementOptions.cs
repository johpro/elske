using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElskeLib.Model
{
    public class CountRequirementOptions
    {
        /// <summary>
        /// For single words, the minimum number of appearances in the source
        /// </summary>
        public int MinUnigramFrequency { get; set; }
        /// <summary>
        /// For single words, the minimum number of documents in the reference collection containing it
        /// </summary>
        public int MinUnigramDocCount { get; set; }
        /// <summary>
        /// For bi-grams, the minimum number of appearances in the source
        /// </summary>
        public int MinBigramFrequency { get; set; }
        /// <summary>
        /// For bi-grams, the minimum number of documents in the reference collection containing it
        /// </summary>
        public int MinBigramDocCount { get; set; }
        /// <summary>
        /// For phrases (three or more words), the minimum number of appearances in the source
        /// </summary>
        public int MinPhraseFrequency { get; set; } = 2;
        /// <summary>
        /// For phrases (three or more words), the minimum number of documents in the reference collection containing it
        /// </summary>
        public int MinPhraseDocCount { get; set; }
    }
}
