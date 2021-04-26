/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using System.Linq;

namespace ElskeLib.Utils
{
    public static class StopWords
    {
        public static readonly string[] CzechStopWords =
        {
            "aby", "ačkoli", "ahoj", "aj", "ale", "anebo", "ani", "ano", "asi", "aspoň", "az", "během", "bez", "beze",
            "blízko", "bohužel", "brzo", "bude", "budem", "budeme", "budes", "budeš", "budete", "budou", "budu", "by",
            "byl", "byla", "byli", "bylo", "byly", "bys", "byt", "čau", "chce", "chceme", "chceš", "chcete", "chci",
            "chtějí", "chtít", "chut'", "chuti", "ci", "clanek", "clanku", "clanky", "co", "coz", "čtrnáct", "čtyři",
            "cz", "dál", "dále", "daleko", "dalsi", "děkovat", "děkujeme", "děkuji", "den", "deset", "design",
            "devatenáct", "devět", "dnes", "do", "dobrý", "docela", "dva", "dvacet", "dvanáct", "dvě", "email", "ho",
            "hodně", "já", "jak", "jako", "jde", "je", "jeden", "jedenáct", "jedna", "jedno", "jednou", "jedou", "jeho",
            "jej", "jeji", "její", "jejich", "jemu", "jen", "jenom", "jeste", "ještě", "jestli", "jestliže", "ji", "jí",
            "jich", "jím", "jimi", "jinak", "jine", "jiz", "jsem", "jses", "jsi", "jsme", "jsou", "jste", "kam", "kde",
            "kdo", "kdy", "kdyz", "když", "ke", "kolik", "kromě", "ktera", "která", "ktere", "které", "kteri", "kteří",
            "kterou", "ktery", "který", "kvůli", "ma", "má", "mají", "málo", "mám", "máme", "máš", "mate", "máte", "mé",
            "mě", "mezi", "mi", "mí", "mit", "mít", "mně", "mnou", "moc", "mohl", "mohou", "moje", "moji", "možná",
            "muj", "můj", "musí", "muze", "může", "my", "na", "nad", "nade", "nam", "nám", "námi", "napiste", "naproti",
            "nas", "nás", "náš", "naše", "nasi", "naši", "ne", "ně", "nebo", "nebyl", "nebyla", "nebyli", "nebyly",
            "něco", "nedělá", "nedělají", "nedělám", "neděláme", "neděláš", "neděláte", "nějak", "nejsi", "nejsou",
            "někde", "někdo", "nemají", "nemáme", "nemáte", "neměl", "němu", "neni", "není", "nestačí", "nevadí", "nez",
            "než", "nic", "nich", "ním", "nimi", "nove", "novy", "nula", "od", "ode", "on", "ona", "oni", "ono", "ony",
            "osm", "osmnáct", "pak", "patnáct", "pět", "po", "pod", "podle", "pokud", "pořád", "potom", "pouze",
            "pozdě", "prave", "pred", "před", "pres", "přes", "přese", "pri", "pro", "proc", "proč", "prosím", "prostě",
            "proti", "proto", "protoze", "protože", "prvni", "pta", "re", "rovně", "se", "sedm", "sedmnáct", "šest",
            "šestnáct", "si", "skoro", "smějí", "smí", "snad", "spolu", "sta", "sté", "sto", "strana", "sve", "svych",
            "svym", "svymi", "ta", "tady", "tak", "take", "takhle", "taky", "takze", "tam", "tamhle", "tamhleto",
            "tamto", "tato", "tě", "tebe", "tebou", "ted'", "tedy", "tema", "ten", "tento", "teto", "ti", "tim",
            "timto", "tipy", "tisíc", "tisíce", "to", "tobě", "tohle", "toho", "tohoto", "tom", "tomto", "tomuto",
            "toto", "třeba", "tři", "třináct", "trošku", "tu", "tuto", "tvá", "tvé", "tvoje", "tvůj", "ty", "tyto",
            "určitě", "uz", "už", "vam", "vám", "vámi", "vas", "vás", "váš", "vase", "vaše", "vaši", "ve", "večer",
            "vedle", "vice", "vlastně", "vsak", "všechno", "všichni", "vůbec", "vy", "vždy", "za", "zač", "zatímco",
            "zda", "zde", "ze", "že", "zpet", "zpravy"
        };

        public static readonly HashSet<string> CzechStopWordsSet = CzechStopWords.ToHashSet();


        public static readonly string[] DanishStopWords =
        {
            "af", "aldrig", "alle", "altid", "andet", "andre", "at", "bagved", "begge", "da", "de", "De", "den",
            "denne", "der", "deres", "det", "dette", "dig", "din", "dog", "du", "efter", "ej", "eller", "en", "end",
            "endnu", "ene", "eneste", "enhver", "et", "få", "fem", "fire", "fjernt", "flere", "fleste", "for", "før",
            "foran", "fordi", "forrige", "fra", "gennem", "god", "han", "hans", "har", "hendes", "her", "hos", "hovfor",
            "hun", "hurtig", "hvad", "hvem", "hver", "hvilken", "hvis", "hvonår", "hvor", "hvordan", "hvorfor",
            "hvorhen", "hvornår", "i", "I", "ikke", "imod", "ind", "ingen", "intet", "ja", "jeg", "jeres", "kan", "kom",
            "kommer", "langsom", "lav", "lidt", "lille", "man", "mand", "mange", "måske", "med", "meget", "mellem",
            "men", "mens", "mere", "mig", "mindre", "nær", "næste", "næsten", "når", "ned", "nede", "nej", "ni",
            "nogen", "noget", "nok", "nu", "ny", "nyt", "og", "op", "oppe", "otte", "over", "på", "rask", "sammen",
            "se", "seks", "ses", "som", "stor", "store", "syv", "temmelig", "ti", "til", "to", "tre", "ud", "uden",
            "udenfor", "under", "var", "ved", "vi"
        };

        public static readonly HashSet<string> DanishStopWordsSet = DanishStopWords.ToHashSet();


        public static readonly string[] DutchStopWords =
        {
            "aan", "aangaande", "aangezien", "achter", "achterna", "af", "afgelopen", "al", "aldaar", "aldus",
            "alhoewel", "alias", "alle", "allebei", "alleen", "als", "alsnog", "altijd", "altoos", "ander", "andere",
            "anders", "anderszins", "behalve", "behoudens", "beide", "beiden", "ben", "beneden", "bent", "bepaald",
            "betreffende", "bij", "binnen", "binnenin", "boven", "bovenal", "bovendien", "bovengenoemd", "bovenstaand",
            "bovenvermeld", "buiten", "daar", "daarheen", "daarin", "daarna", "daarnet", "daarom", "daarop",
            "daarvanlangs", "dan", "dat", "de", "die", "dikwijls", "dit", "door", "doorgaand", "dus", "echter", "een",
            "eer", "eerdat", "eerder", "eerlang", "eerst", "elk", "elke", "en", "enig", "enigszins", "enkel", "er",
            "erdoor", "even", "eveneens", "evenwel", "gauw", "gedurende", "geen", "gehad", "gekund", "geleden",
            "gelijk", "gemoeten", "gemogen", "geweest", "gewoon", "gewoonweg", "haar", "had", "hadden", "hare", "heb",
            "hebben", "hebt", "heeft", "hem", "hen", "het", "hierbeneden", "hierboven", "hij", "hoe", "hoewel", "hun",
            "hunne", "ik", "ikzelf", "in", "inmiddels", "inzake", "is", "je", "jezelf", "jij", "jijzelf", "jou", "jouw",
            "jouwe", "juist", "jullie", "kan", "klaar", "kon", "konden", "krachtens", "kunnen", "kunt", "later",
            "liever", "maar", "mag", "me", "meer", "men", "met", "mezelf", "mij", "mijn", "mijnent", "mijner",
            "mijzelf", "misschien", "mocht", "mochten", "moest", "moesten", "moet", "moeten", "mogen", "na", "naar",
            "nadat", "net", "niet", "noch", "nog", "nogal", "nu", "of", "ofschoon", "om", "omdat", "omhoog", "omlaag",
            "omstreeks", "omtrent", "omver", "onder", "ondertussen", "ongeveer", "ons", "onszelf", "onze", "ook", "op",
            "opnieuw", "opzij", "over", "overeind", "overigens", "pas", "precies", "reeds", "rond", "rondom", "sedert",
            "sinds", "sindsdien", "slechts", "sommige", "spoedig", "steeds", "tamelijk", "te", "tenzij", "terwijl",
            "thans", "tijdens", "toch", "toen", "toenmaals", "toenmalig", "tot", "totdat", "tussen", "uit",
            "uitgezonderd", "vaakwat", "van", "vandaan", "vanuit", "vanwege", "veeleer", "verder", "vervolgens", "vol",
            "volgens", "voor", "vooraf", "vooral", "vooralsnog", "voorbij", "voordat", "voordezen", "voordien",
            "voorheen", "voorop", "vooruit", "vrij", "vroeg", "waar", "waarom", "wanneer", "want", "waren", "was",
            "wat", "we", "weer", "weg", "wegens", "wel", "weldra", "welk", "welke", "wie", "wiens", "wier", "wij",
            "wijzelf", "zal", "ze", "zei", "zelfs", "zichzelf", "zij", "zijn", "zijne", "zo", "zodra", "zonder", "zou",
            "zouden", "zowat", "zulke", "zullen", "zult"
        };

        public static readonly HashSet<string> DutchStopWordsSet = DutchStopWords.ToHashSet();



        public static readonly string[] EnglishStopWordsMinimal =
        {
            "a", "a's", "ain't", "am", "an", "and", "are", "aren't",
            "aren’t", "arent", "as", "at", "b", "be", "been", "being", "but", "by",
            "c", "do", "does", "doesn't", "doesn’t", "doesnt", "doing", "don", "don't", "don’t", "dont", "for", "from",
            "had", "hadn't", "hadn’t", "hadnt", "has", "hasn't", "hasn’t", "hasnt",
            "have", "haven't", "haven’t", "havent", "having", "he", "he'd", "he'll", "he's", "he’d", "he’ll", "he’s",
            "her", "here", "here's", "here’s",
            "hers", "him", "his", "i", "i'd", "i'll", "i'm", "i've", "i’d", "i’ll", "i’m", "i’ve", "im",
            "in", "into", "is", "isn't", "isn’t", "isnt", "it", "it'd", "it'll", "it's", "it’s", "its",
            "ive", "me", "my", "no", "not", "of", "on", "or", "our", "ours", "s", "she", "she'd", "she'll", "she's", "she’d", "she’ll",
            "she’s", "so", "that", "that's", "that’s", "thats",
            "the", "their", "theirs", "them", "then", "there", "there's", "there’s", "these", "they", "they'd",
            "they'll", "they're", "they've", "they’d", "they’ll", "they’re", "they’ve", "theyve", "this",
            "those", "to", "u", "was", "wasn't", "wasn’t", "wasnt", "we", "we'd",
            "we'll", "we're", "we've", "we’d", "we’ll", "we’re", "we’ve", "were", "weren't",
            "weren’t", "werent", "weve", "which", "who", "with", "would", "wouldn't", "wouldn’t", "wouldnt",
            "you", "you'd", "you'll", "you're", "you've", "you’d", "you’ll", "you’re", "you’ve", "your", "yours", "youve"
        };

        public static readonly string[] EnglishStopWords =
        {
            "&gt", "000", "a", "a's", "able", "about", "above", "according", "accordingly", "across", "actually",
            "after", "afterwards", "again", "against", "ain't", "all", "allow", "allows", "almost", "alone", "along",
            "already", "also", "although", "always", "am", "among", "amongst", "an", "and", "another", "any", "anybody",
            "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "are", "aren't",
            "aren’t", "arent", "around", "as", "aside", "ask", "asking", "associated", "at", "b", "back", "be",
            "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being",
            "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "big", "both", "but", "by",
            "c", "c'mon", "c's", "came", "can", "can't", "can’t", "cannot", "cant", "cause", "certain", "certainly",
            "changes", "clearly", "co", "com", "come", "comes", "concerning", "consequently", "consider", "considering",
            "contain", "containing", "contains", "corresponding", "could", "couldn't", "couldn’t", "couldnt", "course",
            "currently", "d", "day", "definitely", "described", "despite", "did", "didn't", "didn’t", "didnt",
            "different", "do", "does", "doesn't", "doesn’t", "doesnt", "doing", "don", "don't", "don’t", "done", "dont",
            "down", "during", "e", "each", "edu", "eg", "either", "else", "elsewhere", "en", "enough",
            "entirely", "especially", "et", "etc", "even", "ever", "every", "everybody", "everyone", "everything",
            "everywhere", "ex", "exactly", "except", "f", "far", "few", "first", "followed",
            "following", "follows", "for", "former", "formerly", "from", "further", "furthermore", "g",
            "get", "gets", "getting", "give", "given", "gives", "go", "goes", "going", "gone", "good", "got", "gotten",
            "great", "h", "had", "hadn't", "hadn’t", "hadnt", "happens", "hardly", "has", "hasn't", "hasn’t", "hasnt",
            "have", "haven't", "haven’t", "havent", "having", "he", "he'd", "he'll", "he's", "he’d", "he’ll", "he’s",
            "hello", "help", "hence", "her", "here", "here's", "here’s", "hereafter", "hereby", "herein", "hereupon",
            "hers", "herself", "hi", "him", "himself", "his", "hither", "hopefully", "how", "how's", "how’s", "howbeit",
            "however", "i", "i'd", "i'll", "i'm", "i've", "i’d", "i’ll", "i’m", "i’ve", "ie", "if", "ignored", "im",
            "immediate", "in", "inasmuch", "inc", "indeed", "indicate", "indicated", "indicates", "inner", "insofar",
            "instead", "into", "inward", "is", "isn't", "isn’t", "isnt", "it", "it'd", "it'll", "it's", "it’s", "its",
            "itself", "ive", "j", "just", "k", "keep", "keeps", "kept", "know", "known", "knows", "l", "last", "lately",
            "later", "latter", "latterly", "least", "less", "lest", "let", "let's", "let’s", "life", "like", "liked",
            "likely", "little", "lol", "look", "looking", "looks", "love", "ltd", "m", "made", "mainly", "make", "man",
            "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might", "more", "moreover", "most", "mostly",
            "much", "must", "mustn't", "mustn’t", "mustnt", "my", "myself", "n", "name", "namely", "nd", "near",
            "nearly", "necessary", "need", "needs", "neither", "never", "nevertheless", "new", "next", "no",
            "nobody", "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "o",
            "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto",
            "or", "other", "others", "otherwise", "ought", "our", "ours", "ourselves", "out", "outside", "over",
            "overall", "own", "p", "particular", "particularly", "per", "perhaps", "placed", "please", "plus",
            "possible", "presumably", "probably", "provides", "q", "que", "quite", "qv", "r", "rather", "rd", "re",
            "really", "reasonably", "regarding", "regardless", "regards", "relatively", "respectively", "right", "s",
            "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed",
            "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously",
            "several", "shall", "shan't", "shan’t", "shant", "she", "she'd", "she'll", "she's", "she’d", "she’ll",
            "she’s", "should", "shouldn't", "shouldn’t", "shouldnt", "since", "so", "some", "somebody",
            "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry",
            "specified", "specify", "specifying", "still", "sub", "such", "sup", "sure", "t", "t's", "take", "taken",
            "taking", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "that's", "that’s", "thats",
            "the", "their", "theirs", "them", "themselves", "then", "thence", "there", "there's", "there’s",
            "thereafter", "thereby", "therefore", "therein", "theres", "thereupon", "these", "they", "they'd",
            "they'll", "they're", "they've", "they’d", "they’ll", "they’re", "they’ve", "theyve", "think",
            "this", "those", "though", "through", "throughout", "thru", "thus", "time", "to", "today",
            "together", "told", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying",
            "u", "un", "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us",
            "use", "used", "useful", "uses", "using", "usually", "uucp", "v", "value", "various", "ve", "very", "via",
            "viz", "vs", "w", "want", "wants", "was", "wasn't", "wasn’t", "wasnt", "watch", "way", "we", "we'd",
            "we'll", "we're", "we've", "we’d", "we’ll", "we’re", "we’ve", "welcome", "well", "went", "were", "weren't",
            "weren’t", "werent", "weve", "what", "what's", "what’s", "whatever", "when", "when's", "when’s", "whence",
            "whenever", "where", "where's", "where’s", "whereafter", "whereas", "whereby", "wherein", "whereupon",
            "wherever", "whether", "which", "while", "whither", "who", "who's", "who’s", "whoever", "whole", "whom",
            "whose", "why", "why's", "why’s", "will", "willing", "wish", "with", "within", "without", "won't", "won’t",
            "wonder", "wont", "would", "wouldn't", "wouldn’t", "wouldnt", "x", "y", "y'all", "years", "yes", "yet",
            "you", "you'd", "you'll", "you're", "you've", "you’d", "you’ll", "you’re", "you’ve", "your", "yours",
            "yourself", "yourselves", "youve", "z", "zero"
        };


        public static readonly HashSet<string> EnglishStopWordsSet = EnglishStopWords.ToHashSet();


        public static readonly string[] FrenchStopWords =
        {
            "a", "à", "â", "abord", "afin", "ah", "ai", "aie", "aime", "ainsi", "allaient", "aller", "allo", "allô",
            "allons", "alors", "ans", "après", "as", "assez", "attendu", "au", "aucun", "aucune", "aucuns", "aujourd",
            "aujourd'hui", "auquel", "aura", "auront", "aussi", "autre", "autres", "aux", "auxquelles", "auxquels",
            "avaient", "avais", "avait", "avant", "ave", "avec", "avez", "avoir", "ayant", "b", "bah", "beaucoup",
            "bien", "bigre", "bon", "bonne", "boum", "bravo", "brrr", "c", "ca", "ça", "car", "ce", "ceci", "cela",
            "celle", "celle-ci", "celle-là", "celles", "celles-ci", "celles-là", "celui", "celui-ci", "celui-là",
            "cent", "cependant", "certain", "certaine", "certaines", "certains", "certes", "ces", "cet", "cette",
            "ceux", "ceux-ci", "ceux-là", "chacun", "chaque", "cher", "chère", "chères", "chers", "chez", "chiche",
            "chut", "ci", "cinq", "cinquantaine", "cinquante", "cinquantième", "cinquième", "clac", "clic", "combien",
            "comme", "comment", "compris", "concernant", "contre", "couic", "crac", "d", "d'une", "da", "dans", "de",
            "debout", "début", "dedans", "dehors", "déjà", "delà", "depuis", "derrière", "des", "dès", "désormais",
            "desquelles", "desquels", "dessous", "dessus", "deux", "deuxième", "deuxièmement", "devant", "devers",
            "devra", "devrait", "différent", "différente", "différentes", "différents", "dire", "dit", "divers",
            "diverse", "diverses", "dix", "dix-huit", "dix-neuf", "dix-sept", "dixième", "doit", "doivent", "donc",
            "dont", "dos", "douze", "douzième", "dring", "droite", "du", "duquel", "durant", "e", "effet", "eh", "elle",
            "elle-même", "elles", "elles-mêmes", "en", "encore", "entre", "envers", "environ", "es", "ès", "essai",
            "est", "et", "étaient", "étais", "était", "etant", "étant", "état", "etc", "été", "étions", "etre", "être",
            "eu", "euh", "eux", "eux-mêmes", "excepté", "f", "façon", "faire", "fais", "faisaient", "faisant", "fait",
            "faites", "faut", "feront", "fi", "flac", "floc", "fois", "font", "force", "g", "gens", "grand", "gros",
            "h", "ha", "haut", "hé", "hein", "hélas", "hem", "hep", "hi", "hier", "ho", "holà", "hop", "hormis", "hors",
            "hou", "houp", "hue", "hui", "huit", "huitième", "hum", "hurrah", "i", "ici", "il", "ils", "importe", "j",
            "j’ai", "jamais", "je", "jour", "jours", "jusqu", "jusque", "juste", "k", "l", "la", "là", "laquelle",
            "las", "le", "lequel", "les", "lès", "lesquelles", "lesquels", "leur", "leurs", "longtemps", "lorsque",
            "lui", "lui-même", "m", "ma", "maint", "maintenant", "mais", "mal", "malgré", "me", "mec", "même", "mêmes",
            "merci", "mère", "mes", "mien", "mienne", "miennes", "miens", "mille", "mince", "mine", "moi", "moi-même",
            "moins", "mois", "mon", "monde", "mot", "moyennant", "n", "n'est", "n’est", "na", "ne", "néanmoins", "neuf",
            "neuvième", "ni", "nombreuses", "nombreux", "nommés", "non", "nos", "notre", "nôtre", "nôtres", "nous",
            "nous-mêmes", "nouveaux", "nul", "o", "ô", "o|", "oh", "ohé", "olé", "ollé", "on", "ont", "onze", "onzième",
            "ore", "ou", "où", "ouf", "oui", "ouias", "oust", "ouste", "outre", "p", "paf", "pan", "par", "parce",
            "parmi", "parole", "partant", "particulier", "particulière", "particulièrement", "pas", "passé", "pendant",
            "personne", "personnes", "peu", "peut", "peuvent", "peux", "pff", "pfft", "pfut", "pièce", "pif", "plein",
            "plouf", "plupart", "plus", "plusieurs", "plutôt", "pouah", "pour", "pourquoi", "premier", "première",
            "premièrement", "près", "proche", "psitt", "puisque", "q", "qu", "qu'il", "qu'on", "qu’il", "qu’on",
            "quand", "quant", "quant-à-soi", "quanta", "quarante", "quatorze", "quatre", "quatre-vingt", "quatrième",
            "quatrièmement", "que", "quel", "quelconque", "quelle", "quelles", "quelqu'un", "quelque", "quelques",
            "quels", "qui", "quiconque", "quinze", "quoi", "quoique", "r", "revoici", "revoilà", "rien", "s", "sa",
            "sacrebleu", "sais", "sans", "sapristi", "sauf", "se", "seize", "selon", "sept", "septième", "sera",
            "seront", "ses", "seulement", "si", "sien", "sienne", "siennes", "siens", "sinon", "six", "sixième", "soi",
            "soi-même", "soir", "soit", "soixante", "son", "sont", "sous", "soyez", "stop", "suis", "suivant", "sujet",
            "sur", "surtout", "t", "t’es", "ta", "tac", "tandis", "tant", "te", "té", "tel", "telle", "tellement",
            "telles", "tels", "tenant", "tes", "tic", "tien", "tienne", "tiennes", "tiens", "toc", "toi", "toi-même",
            "ton", "touchant", "toujours", "tous", "tout", "toute", "toutes", "treize", "trente", "très", "trois",
            "troisième", "troisièmement", "trop", "tsoin", "tsouin", "tu", "u", "un", "une", "unes", "uns", "v", "va",
            "vais", "valeur", "vas", "vé", "vers", "veut", "veux", "via", "vie", "vif", "vifs", "vingt", "vivat",
            "vive", "vives", "vlan", "voici", "voie", "voient", "voilà", "voir", "vont", "vos", "votre", "vôtre",
            "vôtres", "vous", "vous-mêmes", "vrai", "vraiment", "vu", "w", "x", "y", "y’a", "z", "zut"
        };

        public static readonly HashSet<string> FrenchStopWordsSet = FrenchStopWords.ToHashSet();


        public static readonly string[] GermanStopWords =
        {
            "a", "ab", "aber", "ach", "acht", "achte", "achten", "achter", "achtes", "ag", "alle", "allein", "allem",
            "allen", "aller", "allerdings", "alles", "allgemeinen", "als", "also", "am", "an", "andere", "anderen",
            "andern", "anders", "au", "auch", "auf", "aus", "ausser", "außer", "ausserdem", "außerdem", "b", "bald",
            "bei", "beide", "beiden", "beim", "beispiel", "bekannt", "bereits", "besonders", "besser", "besten", "bin",
            "bis", "bisher", "bist", "bitte", "c", "cm", "d", "d.h", "da", "dabei", "dadurch", "dafür", "dagegen",
            "daher", "dahin", "dahinter", "damals", "damit", "danach", "daneben", "dank", "danke", "dann", "daran",
            "darauf", "daraus", "darf", "darfst", "darin", "darüber", "darum", "darunter", "das", "dasein", "daselbst",
            "dass", "daß", "dasselbe", "davon", "davor", "dazu", "dazwischen", "de", "dein", "deine", "deinem",
            "deiner", "dem", "dementsprechend", "demgegenüber", "demgemäss", "demgemäß", "demselben", "demzufolge",
            "den", "denen", "denn", "denselben", "der", "deren", "derjenige", "derjenigen", "dermassen", "dermaßen",
            "derselbe", "derselben", "des", "deshalb", "desselben", "dessen", "deswegen", "dich", "die", "diejenige",
            "diejenigen", "dies", "diese", "dieselbe", "dieselben", "diesem", "diesen", "dieser", "dieses", "dir",
            "doch", "dort", "drei", "drin", "dritte", "dritten", "dritter", "drittes", "du", "durch", "durchaus",
            "dürfen", "dürft", "durfte", "durften", "e", "eben", "ebenso", "eher", "ehrlich", "ei", "ei,", "eigen",
            "eigene", "eigenen", "eigener", "eigenes", "eigentlich", "ein", "einander", "eine", "einem", "einen",
            "einer", "eines", "einfach", "einige", "einigen", "einiger", "einiges", "einmal", "eins", "elf", "en",
            "ende", "endlich", "entweder", "er", "Ernst", "erst", "erste", "ersten", "erster", "erstes", "es", "etwa",
            "etwas", "euch", "euer", "eure", "f", "früher", "fünf", "fünfte", "fünften", "fünfter", "fünftes", "für",
            "g", "gab", "galt", "galten", "ganz", "ganze", "ganzen", "ganzer", "ganzes", "gar", "gedurft", "gegen",
            "gegenüber", "gehabt", "gehen", "geht", "gekannt", "gekonnt", "gelten", "gemacht", "gemocht", "gemusst",
            "genau", "genug", "gerade", "gern", "gesagt", "geschweige", "gewesen", "gewollt", "geworden", "gibt",
            "gilt", "ging", "gleich", "gott", "gross", "groß", "grosse", "große", "grossen", "großen", "grosser",
            "großer", "grosses", "großes", "gut", "gute", "guten", "guter", "gutes", "h", "hab", "habe", "haben",
            "habt", "halt", "hast", "hat", "hatte", "hätte", "hatten", "hätten", "hattest", "hattet", "heisst", "her",
            "heute", "hier", "hin", "hinter", "hoch", "i", "ich", "ihm", "ihn", "ihnen", "ihr", "ihre", "ihrem",
            "ihren", "ihrer", "ihres", "im", "immer", "in", "indem", "infolgedessen", "ins", "irgend", "ist", "j", "ja",
            "jahr", "jahre", "jahren", "je", "jede", "jedem", "jeden", "jeder", "jedermann", "jedermanns", "jedes",
            "jedoch", "jemand", "jemandem", "jemanden", "jene", "jenem", "jenen", "jener", "jenes", "jetzt", "k", "kam",
            "kann", "kannst", "kaum", "kein", "keine", "keinem", "keinen", "keiner", "kleine", "kleinen", "kleiner",
            "kleines", "kommen", "kommt", "können", "könnt", "konnte", "könnte", "konnten", "kurz", "l", "lang",
            "lange", "lassen", "laufen", "leicht", "leide", "leider", "leute", "liebe", "lieber", "los", "m", "machen",
            "macht", "machte", "mag", "magst", "mahn", "mal", "man", "manche", "manchem", "manchen", "mancher",
            "manches", "mann", "mehr", "mein", "meine", "meinem", "meinen", "meiner", "meines", "mensch", "menschen",
            "mich", "mir", "mit", "mittel", "mochte", "möchte", "mochten", "mögen", "möglich", "mögt", "morgen", "muss",
            "muß", "müssen", "musst", "mußt", "müsst", "müßt", "musste", "mussten", "n", "na", "nach", "nachdem",
            "nahm", "natürlich", "ne", "neben", "nein", "neue", "neuen", "neun", "neunte", "neunten", "neunter",
            "neuntes", "nicht", "nichts", "nie", "niemand", "niemandem", "niemanden", "nix", "noch", "nun", "nur", "o",
            "ob", "oben", "oder", "offen", "oft", "oh", "ohne", "p", "paar", "pro", "q", "r", "recht",
            "rechte", "rechten", "rechter", "rechtes", "richtig", "rund", "s", "sa", "sache", "sagen", "sagt", "sagte",
            "sah", "satt", "schlecht", "schon", "sechs", "sechste", "sechsten", "sechster", "sechstes",
            "sehen", "sehr", "sei", "seid", "seien", "sein", "seine", "seinem", "seinen", "seiner", "seines", "seit",
            "seitdem", "selbst", "sich", "sie", "sieben", "siebente", "siebenten", "siebenter", "siebentes", "sind",
            "so", "sogar", "solang", "solche", "solchem", "solchen", "solcher", "solches", "soll", "sollen", "sollst",
            "sollt", "sollte", "sollten", "sondern", "sonst", "soweit", "sowie", "später", "statt", "steht", "t", "tag",
            "tage", "tagen", "tat", "teil", "tel", "tritt", "trotzdem", "tun", "u", "über", "überhaupt", "übrigens",
            "uhr", "um", "und", "und?", "uns", "unser", "unsere", "unserer", "unter", "v", "vergangenen", "via", "viel",
            "viele", "vielem", "vielen", "vielleicht", "vier", "vierte", "vierten", "vierter", "viertes", "vom", "von",
            "vor", "vs", "w", "wahr?", "während", "währenddem", "währenddessen", "wann", "war", "wäre", "waren", "wart",
            "warum", "was", "wegen", "weil", "weiß", "weit", "weiter", "weitere", "weiteren", "weiteres", "welche",
            "welchem", "welchen", "welcher", "welches", "wem", "wen", "wenig", "wenige", "weniger", "weniges",
            "wenigstens", "wenn", "wer", "werde", "werden", "werdet", "weshalb", "wessen", "wie", "wieder", "wieso",
            "will", "willst", "wir", "wird", "wirklich", "wirst", "wo", "woher", "wohin", "wohl", "wollen", "wollt",
            "wollte", "wollten", "worden", "wurde", "würde", "wurden", "würden", "x", "y", "z", "z.b", "zehn", "zehnte",
            "zehnten", "zehnter", "zehntes", "zeit", "zu", "zuerst", "zugleich", "zum", "zunächst", "zur", "zurück",
            "zusammen", "zwanzig", "zwar", "zwei", "zweite", "zweiten", "zweiter", "zweites", "zwischen", "zwölf",
            "laut", "derzeit", "zudem", "000", "fast", "heißt", "bleiben", "klar", "bleibt", "meisten"
        };

        public static readonly HashSet<string> GermanStopWordsSet = GermanStopWords.ToHashSet();


        public static readonly string[] ItalianStopWords =
        {
            "a", "Ã¨", "abbastanza", "accidenti", "ad", "adesso", "affinche", "agli", "ahimÃ¨", "ahime", "ai", "al",
            "alcuna", "alcuni", "alcuno", "all", "alla", "alle", "allo", "allora", "altre", "altri", "altrimenti",
            "altro", "altrui", "anche", "ancora", "anni", "anno", "ansa", "assai", "attesa", "avanti", "avendo",
            "avente", "aver", "avere", "avete", "aveva", "avevano", "avuta", "avute", "avuti", "avuto", "basta", "ben",
            "bene", "benissimo", "berlusconi", "brava", "bravo", "buono", "c", "casa", "caso", "cento", "certa",
            "certe", "certi", "certo", "che", "chi", "chicchessia", "chiunque", "ci", "ciÃ²", "ciascuna", "ciascuno",
            "cima", "cinque", "cio", "cioÃ¨", "cioe", "circa", "citta", "cittÃ", "codesta", "codesti", "codesto",
            "cogli", "coi", "col", "colei", "coll", "coloro", "colui", "come", "comprare", "con", "concernente",
            "consecutivi", "consecutivo", "consiglio", "contro", "cortesia", "cos", "cosa", "cosÃ¬", "cosi", "così",
            "cui", "d", "da", "dagli", "dai", "dal", "dall", "dalla", "dalle", "dallo", "davanti", "de", "degli", "dei",
            "del", "dell", "della", "delle", "dello", "dentro", "detto", "deve", "devo", "di", "dice", "dietro", "dire",
            "dirimpetto", "do", "dopo", "doppio", "dove", "dovra", "dovrÃ", "due", "dunque", "durante", "e", "ecco",
            "ed", "egli", "ella", "eppure", "era", "erano", "esse", "essendo", "esser", "essere", "essi", "ex", "fa",
            "fare", "fatto", "favore", "fin", "finalmente", "finche", "fine", "fino", "forse", "fra", "fuori", "gente",
            "gia", "giÃ", "giacche", "giorni", "giorno", "giu", "gli", "gliela", "gliele", "glieli", "glielo", "gliene",
            "governo", "grande", "grazie", "gruppo", "ha", "hai", "hanno", "ho", "i", "ieri", "il", "improvviso", "in",
            "indietro", "infatti", "insieme", "intanto", "intorno", "invece", "io", "l", "la", "lÃ", "lavoro", "le",
            "lei", "li", "lo", "lontano", "loro", "lui", "lungo", "ma", "macche", "magari", "mai", "male", "malgrado",
            "malissimo", "me", "medesimo", "mediante", "meglio", "meno", "mentre", "mesi", "mezzo", "mi", "mia", "mie",
            "miei", "mila", "miliardi", "milioni", "ministro", "mio", "molta", "molti", "moltissimo", "molto", "mondo",
            "nazionale", "ne", "negli", "nei", "nel", "nell", "nella", "nelle", "nello", "nemmeno", "neppure",
            "nessuna", "nessuno", "niente", "no", "noi", "nome", "non", "nondimeno", "nostra", "nostre", "nostri",
            "nostro", "nove", "nulla", "nuovi", "nuovo", "o", "od", "oggi", "ogni", "ognuna", "ognuno", "oltre",
            "oppure", "ora", "ore", "osi", "ossia", "otto", "paese", "parecchi", "parecchie", "parecchio", "parte",
            "partendo", "peccato", "peggio", "per", "perÃ²", "perchÃ¨", "perche", "perché", "perciÃ²", "percio",
            "perfino", "pero", "però", "persone", "piÃ¹", "piedi", "pieno", "piglia", "piu", "più", "po", "pochissimo",
            "poco", "poi", "poiche", "press", "prima", "primo", "promesso", "proprio", "puÃ²", "puo", "può", "pure",
            "purtroppo", "qua", "qualche", "qualcuna", "qualcuno", "quale", "quali", "qualunque", "quando", "quanta",
            "quante", "quanti", "quanto", "quantunque", "quarto", "quasi", "quattro", "que", "quel", "quella", "quelli",
            "quello", "quest", "questa", "queste", "questi", "questo", "qui", "quindi", "quinto", "riecco", "rispetto",
            "salvo", "sara", "sarÃ", "sarebbe", "scopo", "scorso", "se", "secondo", "seguente", "sei", "sembra",
            "sembrava", "sempre", "senza", "sette", "si", "sia", "siamo", "siete", "so", "solito", "solo", "sono",
            "sopra", "soprattutto", "sotto", "sta", "staranno", "stata", "state", "stati", "stato", "stesso", "su",
            "sua", "subito", "successivo", "sue", "sugli", "sui", "sul", "sull", "sulla", "sulle", "sullo", "suo",
            "suoi", "tale", "talvolta", "tanto", "te", "tempo", "terzo", "ti", "torino", "tra", "tranne", "tre",
            "triplo", "troppo", "tu", "tua", "tue", "tuo", "tuoi", "tutta", "tuttavia", "tutte", "tutti", "tutto",
            "uguali", "ultimo", "un", "una", "uno", "uomo", "va", "vai", "vale", "varia", "varie", "vario", "verso",
            "vi", "via", "vicino", "visto", "vita", "voi", "volta", "volte", "vostra", "vostre", "vostri", "vostro"
        };

        public static readonly HashSet<string> ItalianStopWordsSet = ItalianStopWords.ToHashSet();


        public static readonly string[] NorwegianStopWords =
        {
            "å", "Å", "alle", "andre", "arbeid", "av", "begge", "bort", "bra", "bruke", "da", "denne", "der", "deres",
            "det", "din", "disse", "du", "eller", "en", "ene", "eneste", "enhver", "enn", "er", "et", "få", "fÅ",
            "folk", "for", "før", "fordi", "forsøke", "først", "forsÛke", "fra", "fÛr", "fÛrst", "gå", "gÅ", "gjorde",
            "gjøre", "gjÛre", "god", "ha", "hadde", "han", "hans", "hennes", "her", "hva", "hvem", "hver", "hvilken",
            "hvis", "hvor", "hvordan", "hvorfor", "i", "ikke", "inn", "innen", "kan", "kunne", "lage", "lang", "lik",
            "like", "må", "mÅ", "makt", "mange", "måte", "mÅte", "med", "meg", "meget", "men", "mens", "mer", "mest",
            "min", "mye", "nå", "nÅ", "når", "nÅr", "navn", "nei", "ny", "og", "også", "ogsÅ", "om", "opp", "oss",
            "over", "på", "pÅ", "part", "punkt", "rett", "riktig", "så", "sÅ", "samme", "sant", "si", "siden", "sist",
            "skulle", "slik", "slutt", "som", "start", "stille", "tid", "til", "tilbake", "tilstand", "under", "ut",
            "uten", "være", "vært", "var", "vår", "vÅr", "ved", "verdi", "vi", "vil", "ville", "vite", "vÖre", "vÖrt"
        };

        public static readonly HashSet<string> NorwegianStopWordsSet = NorwegianStopWords.ToHashSet();


        public static readonly string[] PolishStopWords =
        {
            "a", "aby", "ach", "aj", "albo", "ale", "bardziej", "bardzo", "będzie", "bez", "bo", "bowiem", "być", "był",
            "była", "było", "były", "ci", "cię", "ciebie", "co", "czy", "czyli", "daleko", "dla", "dlaczego", "dlatego",
            "do", "dobrze", "dokąd", "dość", "dużo", "dwa", "dwaj", "dwie", "dwoje", "dziś", "dzisiaj", "gdy", "gdyby",
            "gdzie", "go", "i", "ich", "ile", "im", "inny", "innych", "iż", "ja", "ją", "jak", "jakby", "jaki", "jako",
            "je", "jeden", "jedna", "jednak", "jedno", "jego", "jej", "jemu", "jeśli", "jest", "jestem", "jeszcze",
            "jeżeli", "już", "każdy", "kiedy", "kierunku", "kilka", "kto", "która", "które", "którego", "której",
            "który", "których", "którym", "którzy", "ku", "lub", "ma", "mają", "mam", "mi", "między", "mną", "mnie",
            "mogą", "moi", "mój", "moja", "moje", "może", "można", "mu", "my", "na", "nad", "nam", "nami", "nas",
            "nasi", "nasz", "nasza", "nasze", "naszego", "naszych", "natychmiast", "nawet", "nią", "nic", "nich", "nie",
            "niego", "niej", "niemu", "nigdy", "nim", "nimi", "niż", "o", "obok", "od", "około", "on", "ona", "one",
            "oni", "ono", "oraz", "owszem", "po", "pod", "ponieważ", "poza", "przed", "przede", "przedtem", "przez",
            "przy", "również", "są", "sam", "sama", "się", "skąd", "sobie", "swoje", "ta", "tak", "taki", "takie",
            "także", "tam", "te", "tego", "tej", "ten", "też", "to", "tobą", "tobie", "tu", "tutaj", "twoi", "twój",
            "twoja", "twoje", "ty", "tych", "tylko", "tym", "u", "w", "wam", "wami", "was", "wasi", "wasz", "wasza",
            "wasze", "we", "więc", "wiele", "wielu", "właśnie", "wszystkich", "wszystkim", "wszystko", "wtedy", "wy",
            "z", "za", "żaden", "zawsze", "ze", "że"
        };

        public static readonly HashSet<string> PolishStopWordsSet = PolishStopWords.ToHashSet();


        public static readonly string[] PortugeseStopWords =
        {
            "a", "à", "acerca", "adeus", "agora", "aí", "ainda", "além", "algmas", "algo", "algumas", "alguns", "ali",
            "ambos", "ano", "anos", "antes", "ao", "aos", "apenas", "apoio", "apontar", "após", "aquela", "aquelas",
            "aquele", "aqueles", "aqui", "aquilo", "área", "as", "às", "assim", "até", "atrás", "através", "baixo",
            "bastante", "bem", "boa", "boas", "bom", "bons", "breve", "cá", "cada", "caminho", "catorze", "cedo",
            "cento", "certamente", "certeza", "cima", "cinco", "coisa", "com", "como", "comprido", "conhecido",
            "conselho", "contra", "corrente", "custa", "da", "dá", "dão", "daquela", "daquelas", "daquele", "daqueles",
            "dar", "das", "de", "debaixo", "demais", "dentro", "depois", "desde", "desligado", "dessa", "dessas",
            "desse", "desses", "desta", "destas", "deste", "destes", "deve", "devem", "deverá", "dez", "dezanove",
            "dezasseis", "dezassete", "dezoito", "dia", "diante", "direita", "diz", "dizem", "dizer", "do", "dois",
            "dos", "doze", "duas", "dúvida", "e", "é", "ela", "elas", "ele", "eles", "em", "embora", "enquanto",
            "então", "entre", "era", "és", "essa", "essas", "esse", "esses", "esta", "está", "estado", "estão", "estar",
            "estará", "estas", "estás", "estava", "este", "estes", "esteve", "estive", "estivemos", "estiveram",
            "estiveste", "estivestes", "estou", "eu", "exemplo", "faço", "falta", "fará", "favor", "faz", "fazeis",
            "fazem", "fazemos", "fazer", "fazes", "fazia", "fez", "fim", "final", "foi", "fomos", "for", "fora",
            "foram", "forma", "foste", "fostes", "fui", "geral", "grande", "grandes", "grupo", "há", "hoje", "hora",
            "horas", "iniciar", "inicio", "ir", "irá", "isso", "ista", "iste", "isto", "já", "lá", "lado", "ligado",
            "local", "logo", "longe", "lugar", "maior", "maioria", "maiorias", "mais", "mal", "mas", "máximo", "me",
            "meio", "menor", "menos", "mês", "meses", "mesmo", "meu", "meus", "mil", "minha", "minhas", "momento",
            "muito", "muitos", "na", "nada", "não", "naquela", "naquelas", "naquele", "naqueles", "nas", "nem",
            "nenhuma", "nessa", "nessas", "nesse", "nesses", "nesta", "nestas", "neste", "nestes", "nível", "no",
            "noite", "nome", "nos", "nós", "nossa", "nossas", "nosso", "nossos", "nova", "novas", "nove", "novo",
            "novos", "num", "numa", "número", "nunca", "o", "obra", "obrigada", "obrigado", "oitava", "oitavo", "oito",
            "onde", "ontem", "onze", "os", "ou", "outra", "outras", "outro", "outros", "para", "parece", "parte",
            "partir", "paucas", "pegar", "pela", "pelas", "pelo", "pelos", "perto", "pessoas", "pode", "pôde", "podem",
            "poder", "poderá", "podia", "põe", "põem", "ponto", "pontos", "por", "porque", "porquê", "posição",
            "possível", "possivelmente", "posso", "pouca", "pouco", "poucos", "povo", "primeira", "primeiras",
            "primeiro", "primeiros", "promeiro", "própria", "próprias", "próprio", "próprios", "próxima", "próximas",
            "próximo", "próximos", "puderam", "quáis", "qual", "qualquer", "quando", "quanto", "quarta", "quarto",
            "quatro", "que", "quê", "quem", "quer", "quereis", "querem", "queremas", "queres", "quero", "questão",
            "quieto", "quinta", "quinto", "quinze", "relação", "sabe", "sabem", "saber", "são", "se", "segunda",
            "segundo", "sei", "seis", "sem", "sempre", "ser", "seria", "sete", "sétima", "sétimo", "seu", "seus",
            "sexta", "sexto", "sim", "sistema", "sob", "sobre", "sois", "somente", "somos", "sou", "sua", "suas", "tal",
            "talvez", "também", "tanta", "tantas", "tanto", "tão", "tarde", "te", "tem", "têm", "temos", "tempo",
            "tendes", "tenho", "tens", "tentar", "tentaram", "tente", "tentei", "ter", "terceira", "terceiro", "teu",
            "teus", "teve", "tipo", "tive", "tivemos", "tiveram", "tiveste", "tivestes", "toda", "todas", "todo",
            "todos", "trabalhar", "trabalho", "três", "treze", "tu", "tua", "tuas", "tudo", "último", "um", "uma",
            "umas", "uns", "usa", "usar", "vai", "vais", "valor", "vão", "vários", "veja", "vem", "vêm", "vens", "ver",
            "verdade", "verdadeiro", "vez", "vezes", "viagem", "vindo", "vinte", "você", "vocês", "vos", "vós", "vossa",
            "vossas", "vosso", "vossos", "zero"
        };

        public static readonly HashSet<string> PortugeseStopWordsSet = PortugeseStopWords.ToHashSet();


        public static readonly string[] SpanishStopWords =
        {
            "a", "actualmente", "adelante", "además", "afirmó", "agregó", "ahí", "ahora", "al", "algo", "algún",
            "alguna", "algunas", "alguno", "algunos", "alrededor", "ambos", "ampleamos", "añadió", "año", "ante",
            "anterior", "antes", "apenas", "aproximadamente", "aquel", "aquellas", "aquellos", "aqui", "aquí", "arriba",
            "aseguró", "así", "atras", "aún", "aunque", "ayer", "bajo", "bastante", "bien", "buen", "buena", "buenas",
            "bueno", "buenos", "cada", "casa", "casi", "caso", "cerca", "cierta", "ciertas", "cierto", "ciertos",
            "cinco", "comentó", "como", "cómo", "con", "conocer", "conseguimos", "conseguir", "considera", "consideró",
            "consigo", "consigue", "consiguen", "consigues", "contra", "cosas", "creo", "cual", "cuales", "cualquier",
            "cuando", "cuanto", "cuatro", "cuenta", "d", "da", "dado", "dan", "dar", "de", "de…", "debe", "deben",
            "debido", "decir", "dejó", "del", "demás", "dentro", "desde", "después", "día", "días", "dice", "dicen",
            "dicho", "dieron", "diferente", "diferentes", "dijeron", "dijo", "dio", "donde", "dos", "durante", "e",
            "ejemplo", "el", "él", "ella", "ellas", "ello", "ellos", "embargo", "empleais", "emplean", "emplear",
            "empleas", "empleo", "en", "encima", "encuentra", "entonces", "entre", "era", "eramos", "eran", "eras",
            "eres", "es", "esa", "esas", "ese", "eso", "esos", "esta", "está", "ésta", "estaba", "estaban", "estado",
            "estais", "estamos", "estan", "están", "estar", "estará", "estas", "éstas", "este", "éste", "esto", "estos",
            "éstos", "estoy", "estuvo", "ex", "existe", "existen", "explicó", "expresó", "falta", "favor", "fin", "fue",
            "fuera", "fueron", "fui", "fuimos", "gente", "gracias", "gran", "grandes", "gueno", "ha", "haber", "había",
            "habían", "habrá", "hace", "haceis", "hacemos", "hacen", "hacer", "hacerlo", "haces", "hacia", "haciendo",
            "hago", "han", "hasta", "hay", "haya", "he", "hecho", "hemos", "hicieron", "hizo", "hola", "hora", "hoy",
            "hubo", "igual", "incluso", "indicó", "informó", "intenta", "intentais", "intentamos", "intentan",
            "intentar", "intentas", "intento", "ir", "junto", "la", "lado", "largo", "las", "le", "les", "llegó",
            "lleva", "llevar", "lo", "los", "luego", "lugar", "m", "mal", "manera", "manifestó", "mas", "más", "mayor",
            "me", "mediante", "mejor", "mencionó", "menos", "mi", "mí", "mientras", "minutos", "mio", "mis", "misma",
            "mismas", "mismo", "mismos", "modo", "momento", "mucha", "muchas", "mucho", "muchos", "muy", "nada",
            "nadie", "ni", "ningún", "ninguna", "ningunas", "ninguno", "ningunos", "no", "nos", "nosotras", "nosotros",
            "nuestra", "nuestras", "nuestro", "nuestros", "nueva", "nuevas", "nuevo", "nuevos", "nunca", "o", "ocho",
            "otra", "otras", "otro", "otros", "país", "para", "parece", "parte", "partir", "pasada", "pasado", "pero",
            "pesar", "poca", "pocas", "poco", "pocos", "podeis", "podemos", "poder", "podrá", "podrán", "podria",
            "podría", "podriais", "podriamos", "podrian", "podrían", "podrias", "poner", "por", "porque", "posible",
            "primer", "primera", "primero", "primeros", "principalmente", "propia", "propias", "propio", "propios",
            "próximo", "próximos", "pudo", "pueda", "puede", "pueden", "puedo", "pues", "q", "que", "qué", "quedó",
            "queremos", "quien", "quién", "quienes", "quiere", "quiero", "realizado", "realizar", "realizó", "respecto",
            "sabe", "sabeis", "sabemos", "saben", "saber", "sabes", "salud", "se", "sé", "sea", "sean", "según",
            "segunda", "segundo", "seguro", "seis", "señaló", "ser", "será", "serán", "sería", "si", "sí", "sido",
            "siempre", "siendo", "siete", "sigue", "siguiente", "sin", "sino", "sobre", "sois", "sola", "solamente",
            "solas", "solo", "sólo", "solos", "somos", "son", "soy", "su", "sus", "tal", "también", "tampoco", "tan",
            "tanto", "tendrá", "tendrán", "teneis", "tenemos", "tener", "tenga", "tengo", "tenía", "tenido", "tercera",
            "ti", "tiempo", "tiene", "tienen", "toda", "todas", "todavía", "todo", "todos", "total", "trabaja",
            "trabajais", "trabajamos", "trabajan", "trabajar", "trabajas", "trabajo", "tras", "trata", "través", "tres",
            "tu", "tuvo", "tuyo", "última", "últimas", "ultimo", "último", "últimos", "un", "una", "unas", "uno",
            "unos", "usa", "usais", "usamos", "usan", "usar", "usas", "uso", "usted", "va", "vais", "valor", "vamos",
            "van", "varias", "varios", "vaya", "veces", "ver", "verdad", "verdadera", "verdadero", "vez", "vida", "vos",
            "vosotras", "vosotros", "voy", "y", "ya", "yo"
        };

        public static readonly HashSet<string> SpanishStopWordsSet = SpanishStopWords.ToHashSet();

        public static readonly string[] EmojiStopWords =
        {
            "😂", "🔥", "😭", "❤️", "🙏", "🥺", "💥", "🤣", "😍", "🏻", "@", "🏾", "🏽", "✨", "😎", "🚨", "👇",
            "+", "❤", "♂️", "🤔", "♀️", "✊", "🥰", "🏼", "💔", "😊", "👍", "👀", "👏", "👉", "🇺", "🤦", "💪", "‼️",
            "🇸", "🕯", "💯", "💕", "🇳", "😩", "💜", "💙", "🤷", "😔", "🙄", "🇬", "😳", "🙌", "👈", "🎉", "💀", "•",
            "😈", "📸", "😉", "💦", "💚", "🤩", "♥️", "😘", "✅", "🖤", "😅", "🥳", "💖", "😁", "🥴", "🤙", "👌", "~",
            "😌", "🔄", "😢", "⚠️", "🤗", "🤍", "😏", "💗", "💛", "😫", "📢", "🗣", "🌟", "👑", "🔁", "👊", "😡", "🏆",
            "🏿", "😆", "😱", "😒", "☺️", "🇮", "🙂", "😋", "♡", "🙃", "🤯", "🔴", "💞", "🤤", "🥵", "🔃", "⃣", "😤",
            "❣️", "😜", "🎶", "🤪", "😬", "🤬", "💋", "⬇️", "%", "🤘", "⚡", "🍆", "🇦", "🤡", "🙈", "🤧", "😷", "💰",
            "😀", "⚡️", "➡️", "🐰", "😇", "🤞", "🍑", "🇪", "🤝", "💫", "🇹", "🌸", "🎁", "$", "💓", "😐", "💣", "🤟",
            "🎂", "✌", "🇷", "🔗", "🇧", "😞", "❌", "😃", "😄", "🎥", "❗", "🚀", "❗️", "🤭", "🌹", "✌️", "→", "🤨",
            "☹️", "📣", "☀️", "🇨", "🚔", "📺", "🇲", "🧡", "🧐", "💃", "🇵", "🦋", "😑", "👅", "🌈", "😪", "🤮", "🎊",
            "🤫", "🌊", "🕊", "⏱", "⭐️", "🤓", "🇭", "😹", "☠️", "🌎", "💘", "💎", "💐", "👩", "✔️", "😥", "⚽️", "🥁",
            "🇰", "🙋", "😚", "_", "🤢", "📍", "👋", "⠀", "😕", "🐶", "📌", "😝", "📷", "🎯", "🔔", "✋", "💸", "⭐",
            "🦁", "🐐", "😛", "😻", "▶️", "🎧", "😖", "😣", "👻", "🎬", "😠", "🙆", "🆘", "🇩", "❄️", "💩", "🔊", "😴",
            "🌼", "😓", "🐱", "✰", "🎈", "♥", "⛈", "★", "🎵", "🦠", "🐥", "🖕", "🌞", "🏃", "🐯", "🇱", "▪️", "🍀",
            "👁", "📱", "👨", "😼", "🔞", "🐻", "⏰", "🌷", "🥂", "🐾", "🌻", "🌍", "👄", "🌚", "🌴", "🔵", "🌱", "🌺",
            "😲", "💝", "🤑", "🐹", "🤚", "🕺", "👎", "🗳", "🤲", "✈️", "^^", "☑️", "🎤", "🏴", "📈", "💨", "🌿", "💵",
            "🥲", "〖", "🤺", "〗", "😮", "🎃", "⁉️", "🌙", "💌", "🎀", "🚶", "🤠", "🥱", "🍿", "👤", "🍾", "🍻", "🍃",
            "🚫", "🙇", "💅", "🏀", "☁️", "💁", "🔹", "🦅", "♻️", "🔸", "$$", "🥇", "💉", "🇾", "🐺", "🖐", "✔", "⚔️",
            "💻", "🇿", "😟", "🚩", "🍓", "🩺", "🙁", "🐍", "⚪️", "🌏", "🧚", "🍒", "￼", "📹", "⤵️", "❓", "🏈", "󠁧",
            "󠁢", "󠁿", "🎨", "🦊", "🥶", "🏛", "🤐", "😶", "⚽", "🔪", "‼", "👆", "📲", "▶", "👂", "🛑", "😰", "❞",
            "💡", "☕️", "🥀", "🇫", "🦉", "🇴", "💟", "🐨", "📚", "📝", "🏁", "💬", "✍", "🎙", "🧍", "👮", "🙊", "💍",
            "☝", "✍️", "🌐", "✝️", "🎓", "🙅", "🔫", "🍷", "™", "📊", "☺", "^", "😗", "🍂", "🍊", "💭", "🌪", "🌀",
            "🎮", "🐷", "×", "😯", "😵", "🔑", "🤎", "🔒", "©", "🎸", "💧", "🐝", "😺", "👸", "📰", "🇯", "🍁", "☆",
            "🤏", "👼", "➡", "⏳", "󠁳", "👿", "🎼", "❝", "🍺", "☝️", "🩸", "☕", "😙", "⬆️", "🤥", "🏅", "👹", "💤",
            "🦇", "🏡", "🍭", "🐕", "⠀⠀", "🤸", "🧸", "🔮", "🤕", "🌳", "🐸", "🍕", "⁣", "☘️", "​", "🐼", "⚠", "🍬",
            "😨", "🤖", "⚖️", "📖", "➫", "🧵", "🔍", "🍫", "🛒", "🐧", "👽", "🐈", "🗓", "♦️", "🤒", "🍰", "🍎", "🌧",
            "🌠", "⚕️", "🧿", "📻", "●", "❕", "❣", "☮️", "😽", "🧠", "😿", "💆", "·", "🤛", "🇻", "💿", "📩", "🧢",
            "😸", "#", "🐿", "👙", "📞", "👶", "🔜", "🔱", "🐦", "🍪", "⛽️", "🧨", "🥊", "🥃", "🔻", "🐢", "―", "🍄",
            "💢", "👧", "🏠", "🧘", "⚫️", "°", "☎️", "🤜", "🌲", "😧", "🏹", "🚂", "🦄", "🌝", "🧼", "🏏", "󠁣", "󠁴",
            "🌾", "🔶", "🚗", "🐀", "🥚", "⌚", "」", "📽", "🦍", "👦", "⛔", "🔎", "🍩", "™️", "⁠", "😾", "🦜", "🚦",
            "🐇", "__", "📅", "🎞", "🔝", "🐘", "✖️", "⚪", "❄", "🎩", "⚾️", "🇼", "🍌", "🐳", "🧻", "♨️", "🍳", "🌅",
            "🙀", "🐎", "🛎", "🗿", "¦¦", "☄️", "++", "🆕", "♠️", "󠁥", "󠁮", "🌌", "🔺", "🐑", "🇽", "🔈", "💊", "♪",
            "👗", "🥈", "♾", "⛔️", "👐", "♡︎", "🐣", "☔️", "🐉", "✧", "💄", "🍼", "🆚", "👣", "🎄", "🍯", "′′", "🎇",
            "🌃", "🗽", "▫️", "🐋", "🤳", "⛓", "🍔", "🍹", "⬇", "🔘", "🧀", "👥", "🖇", "🍸", "🐅", "⚒️", "♫", "🚮",
            "🏫", "🏷", "✓", "🦈", "◾", "🧑", "🔷", "⋆", "🦢", "📋", "🐬", "🙉", "🥉", "🛸", "☹", "　", "🌬", "🐴", "⬅️",
            "👟", "☠", "🐟", "🕷", "⏩", "~~", "👕", "😦", "🔐", "🖼", "🍋", "📆", "🍞", "📦", "£", "🧞", "📉", "🤴",
            "🕸", "👺", "🗡", "🐮", "󠁷", "󠁬", "👭", "⚜️", "🖖", "🦖", "⚘", "❛", "⏱️", "🍉", "🐽", "🎆", "📎", "˚",
            "🚬", "👠", "💲", "🧟", "🪐", "€", "☀", "🎭", "₹", "🧁", "❜", "🔌", "🟢", "⚰️", "🏐", "✏", "🚴", "꒱", "☑",
            "🎻", "🦌", "🍦", "🕒", "🔋", "🚘", "🐛", "↳", "🔽", "🌶", "🗑", "🌕", "🦆", "➖", "📨", "🥛", "✦", "🎹",
            "🍽", "🌵", "☄", "🔨", "⭕️", "🍵", "🐠", "◽", "🔓", "💳", "✏️", "❥", "🍇", "🔂", "🌋", "✉️", "🚑", "🌑",
            "🚚", "🔙", "🟡", "🧎", "¿", "꒰", "｡", "🔟", "🌄", "💠", "👯", "🦾", "🏇", "⚫", "🧤", "🏝", "⚔", "¯", "©️",
            "🔰", "⭕", "⏬", "📀", "　　", "👬", "➕", "🧜", "🕳", "˒", "🌜", "🏄", "🛍", "🚿", "🖥", "🎺", "🥕", "🎟", "`",
            "🏔", "🌛", "🤌", "🆓", "🤰", "🏟", "🦯", "🐊", "⛄️", "🌩", "🥩", "🦶", "❎", "✿", "🎷", "♂", "👃", "👾",
            "📄", "🐖", "🛠", "🏥", "🌤", "📡", "🧙", "🎫", "🛫", "】", "🕵", "🚒", "🐙", "🕛", "「", "🥑", "🧪", "📼",
            "🥙", "🍍", "💷", "🐒", "🐔", "❶", "❷", "📃", "✂️", "👫", "🎾", "🍏", "🌽", "█", "🥧", "🔦", "▪", "🏒",
            "🛡", "🔬", "🥜", "🥅", "📬", "🏎", "🔆", "🎅", "☞", "🏖", "🐭", "♀", "📥", "🍟", "🍜", "✩", "˓", "🖋",
            "⛈️", "🦰", "🧬", "🧹", "›", "🚪", "🦀", "🗞", "💴", "✈", "🎲", "🥞", "💼", "🐄", "🛌", "🎗", "♟️", "☂️",
            "🚄", "🔛", "➜", "🏌", "🔅", "🥾", "🍚", "🍨", "🟥", "🍗", "⌛️", "🍝", "⁣⁣", "☔", "𝐈", "༘", "🕕", "❀",
            "⚙️", "✎", "▪︎", "・", "♥︎", "⛰️", "🥒", "🚙", "🍅", "🚲", "📜", "🥗", "🚁", "─", "🥘", "⚒", "》", "🥤", "🥟",
            "✊️", "🍴", "☪️", "🃏", "【", "🧔", "🥸", "¡", "⛳️", "🕓", "🧏", "ⓜ️", "⁦", "🏉", "◼️", "🐆", "🐓", "👔",
            "🆔", "◀️", "🏮", "≠", "┊", "🏊", "🚧", "🦙", "👖", "👛", "🧊", "🎡", "↘️", "🧛", "⌛", "⛱", "➳", "⁩", "🏰",
            "⚓", "💇", "🕋", "🐂", "🆙", "🥯", "🍧", "🐪", "☃️", "🟠", "〞", "📧", "►", "🐲", "🦸", "🌮", "🛁", "❇️",
            "🏦", "⏪", "。", "🥪", "𝐚", "█　", "🔳", "🏋", "⚓️", "👰", "‣", "🐏", "📮", "🎱", "☾", "🙎", "𝐒", "🔖",
            "☟︎", "🥖", "🧦", "〝", "⛅", "☣️", "🌭", "❸", "🏨", "⏲️", "🦦", "🥬", "〽️", "↓", "🚽", "𝙨", "📕", "🔭",
            "💮", "❦", "𝟲", "✴️", "🐩", "🟤", "🎣", "〰️", "🥦", "🎪", "↻", "👓", "🐞", "🕶", "🥔", "↬", "🪙", "🐌",
            "𝗮", "🗨", "🥥", "⏸", "🦴"
        };

        public static readonly HashSet<string> EmojiStopWordsSet = EmojiStopWords.ToHashSet();

        public static readonly string[] TwitterStopWords =
        {
            "twitter", "retweet", "tweet", "trending", "breaking", "rt", "http", "https", "t", "co", "my", "&", "…",
            "-", "—", "t…", "gt"
        };

        public static readonly string[] TwitterEnglishStopWords =
            TwitterStopWords.Concat(EnglishStopWords)
                .Concat(EmojiStopWords)
                .Concat(Enumerable.Range(0, 32).Select(i => i.ToString())).ToArray();


        public static readonly string[] PunctuationStopWords =
            Tokenizer.PunctuationChars.Select(c => c.ToString()).ToArray();


        public static readonly string[] DigitsStopWords = Enumerable.Range(0, 32).Select(i => i.ToString()).ToArray();
    }
}