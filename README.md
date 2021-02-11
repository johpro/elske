# ELSKE - Efficient Large-Scale Keyphrase Extraction
 
ELSKE is a library that extracts important keyphrases from text sources.
It aims at providing an efficient implementation for extracting keyphrases
from both individual documents and (micro-)document collections such as tweets.
The library is written in C# and runs under .NET 5.

## Introduction

Automatically extracting descriptive words (keywords) or phrases (keyphrases)
from documents is a popular way to summarize their content.
However, most of the existing algorithms and libraries for keyword extraction
focus on individual documents, are computationally expensive,
or support only short keyphrases with up to two or three words.
In natural language processing, accuracy is often significantly more important than efficiency,
but in several use cases that either require near-instant results (interactivity)
or deal with large amounts of data within short time frames ('Big Data'),
it can be infeasible to apply time-consuming approaches.
ELSKE performs keyphrases extraction efficiently even on large collections.
One of the special features of ELSKE is that it can also recognize
longer phrases of interest if they appear unusually often.
In addition, it natively supports the extraction of keyphrases
from entire collections, not only individual documents.
These features are particularly relevant for the analysis
of larger micro-document collections (e.g., tweets).
Despite its focus on efficiency, ELSKE also performs
competitively on traditional keyword extraction benchmarks.

## Quick Start

ELSKE needs a set of sample documents (the *reference collection*)
which it will use to learn the weights of the terms based on how
frequent they usually are. For example, if you have a bunch of
text files in the folder `/path/to/documents` and you want to extract keyphrases
from one or several such documents, you can do this:

```csharp
var elske = KeyphraseExtractor.CreateFromFolder("/path/to/documents");
//extract the top 10 keyphrases of a single document
var phrases = elske.ExtractPhrases("this is one document", 10);
//extract the top 10 keyphrases of several documents
var docs = new[]{"this is one document", "this is another"};
phrases = elske.ExtractPhrases(docs, 10);
```

Instead of building the model from your own data, you can also download and use one of the provided [models](models).
This is an example with one that was created from English news articles:

```csharp
var elske = KeyphraseExtractor.FromFile("en-news.elske");
var phrases = elske.ExtractPhrases("this is an article", 10);
```

## How Does It Work?

Based on the number of top keyphrases you want to extract,
ELSKE applies several heuristics to extract a set of candidate keyphrases
efficiently without having to rely on computationally more expensive techniques such as part-of-speech tagging.
It scores and ranks these candidates according to the PF-IDF scheme,
which is an adjusted version of the popular TF-IDF scheme
that has been adapted to the analysis of larger documents or document collections.
If you want to find out more about how ELSKE works, you can read the paper at ...

## Reference

Please cite the following paper when you use ELSKE in your work:

```
@article{Knittel21Elske,
  title={ELSKE: Efficient Large-Scale Keyphrase Extraction},
  author={Knittel, Johannes and Koch, Steffen and Ertl, Thomas},
  journal={arXiv preprint arXiv:2009.xxx},
  year={2021}
}
```


## License
ELSKE is MIT-licensed.