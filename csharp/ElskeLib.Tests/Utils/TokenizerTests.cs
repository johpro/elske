/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Diagnostics;
using System.Linq;
using ElskeLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ElskeLib.Tests.Utils
{
    [TestClass()]
    public class TokenizerTests
    {
        const string Article =
            "Three people seriously injured jumping into sea at Dorset beach\r\n\r\n" +
            "Three people have been seriously injured jumping off cliffs into the sea at a beach in Dorset.\r\n" +
            "Air ambulances landed at the scene at Durdle Door on Saturday afternoon, " +
            "and police are now asking people to stay away from the popular tourist spot, near Lulworth.\r\n\r\n" +
            "A post on Poole police’s Facebook page said people had been jumping from the arch of rocks at the beach, " +
            "and videos posted on other social media networks show people climbing and making the 200ft leap.\r\n" +
            "The well-known limestone arch at Durdle Door beach, Dorset – and people enjoying the sand and sea " +
            "earlier on Saturday.\r\n\r\n" +
            "Images posted on social media by Purbeck police show helicopters landing on the sand and crowds " +
            "leaving the beach en masse as the area was evacuated.\r\n\r\n" +
            "HM Coastguard and the RNLI are helping to clear the area after police were called at around 3.45pm.\r\n\r\n" +
            "Ch Insp Claire Phillips, of Dorset police, said: “We have had to close the beach at Durdle Door to " +
            "allow air ambulances to land. As a result, we are evacuating the beach and the surrounding cliff area.\r\n\r\n" +
            "“I am urging people to leave the area to enable emergency services to treat the injured people.”\r\n\r\n" +
            "Pictures taken earlier on Saturday showed the beach busy as the public were reminded to practise " +
            "physical distancing in the good weather following the relaxation of coronavirus lockdown restrictions.";
        

        [TestMethod()]
        public void SplitSpacesTest()
        {
            var res = string.Join('|', Article.SplitSpaces());

            const string expected =
                "Three|people|seriously|injured|jumping|into|sea|at|Dorset|beach|Three|people|have|been|seriously|injured|" +
                "jumping|off|cliffs|into|the|sea|at|a|beach|in|Dorset.|Air|ambulances|landed|at|the|scene|at|Durdle|Door|" +
                "on|Saturday|afternoon,|and|police|are|now|asking|people|to|stay|away|from|the|popular|tourist|spot,|near|" +
                "Lulworth.|A|post|on|Poole|police’s|Facebook|page|said|people|had|been|jumping|from|the|arch|of|rocks|at|the|" +
                "beach,|and|videos|posted|on|other|social|media|networks|show|people|climbing|and|making|the|200ft|leap.|The|" +
                "well-known|limestone|arch|at|Durdle|Door|beach,|Dorset|–|and|people|enjoying|the|sand|and|sea|earlier|on|Saturday.|" +
                "Images|posted|on|social|media|by|Purbeck|police|show|helicopters|landing|on|the|sand|and|crowds|leaving|the|beach|en|" +
                "masse|as|the|area|was|evacuated.|HM|Coastguard|and|the|RNLI|are|helping|to|clear|the|area|after|police|were|called|at|" +
                "around|3.45pm.|Ch|Insp|Claire|Phillips,|of|Dorset|police,|said:|“We|have|had|to|close|the|beach|at|Durdle|Door|to|allow|" +
                "air|ambulances|to|land.|As|a|result,|we|are|evacuating|the|beach|and|the|surrounding|cliff|area.|“I|am|urging|people|to|" +
                "leave|the|area|to|enable|emergency|services|to|treat|the|injured|people.”|Pictures|taken|earlier|on|Saturday|showed|" +
                "the|beach|busy|as|the|public|were|reminded|to|practise|physical|distancing|in|the|good|weather|following|the|" +
                "relaxation|of|coronavirus|lockdown|restrictions.";
            Trace.WriteLine(res);

            Assert.AreEqual(expected, res);

        }

        [TestMethod()]
        public void CleanTweetsTest()
        {
            const string tweet = "RT @realdonaldtrump: this is HILARIOUS!! :) #trump #ddd https://asf.t.co #";

            var splits = tweet.SplitSpaces().ToArray();

            var res = string.Join(' ', splits.CleanTweets(true, false, false));
            Trace.WriteLine(res);
            Assert.AreEqual("RT @realdonaldtrump: this is HILARIOUS!! :) https://asf.t.co", res);

            res = string.Join(' ', splits.CleanTweets(false, true, false));
            Trace.WriteLine(res);
            Assert.AreEqual("RT this is HILARIOUS!! :) #trump #ddd https://asf.t.co #", res);
            
            res = string.Join(' ', splits.CleanTweets(false, false, true));
            Trace.WriteLine(res);
            Assert.AreEqual("RT @realdonaldtrump: this is HILARIOUS!! :) #trump #ddd #", res);
            
            res = string.Join(' ', splits.CleanTweets(true, true, true));
            Trace.WriteLine(res);
            Assert.AreEqual("RT this is HILARIOUS!! :)", res);

            res = string.Join(' ', splits.CleanTweets(false, true, true, false, true));
            Trace.WriteLine(res);
            Assert.AreEqual("RT this is HILARIOUS!! :) trump ddd", res);

            res = string.Join(' ', splits.CleanTweets(false, true, true, true, true));
            Trace.WriteLine(res);
            Assert.AreEqual("this is HILARIOUS!! :) trump ddd", res);
        }

        [TestMethod()]
        public void TokenizeTest()
        {
            Assert.IsTrue(Article.SplitSpaces().Count() <
                Article.SplitSpaces().Tokenize().Count());

            var res = string.Join('|', Article.SplitSpaces().Tokenize());
            
            Trace.WriteLine(res);

            var res2= string.Join('|', Article.Tokenize());
            Assert.AreEqual(res, res2);

            Assert.AreEqual("Three|people|seriously|injured|jumping|into|sea|at|Dorset|beach|Three|people|have|been|seriously|injured|jumping|off|cliffs|into|the|sea|at|a|beach|in|Dorset|.|Air|ambulances|landed|at|the|scene|at|Durdle|Door|on|Saturday|afternoon|,|and|police|are|now|asking|people|to|stay|away|from|the|popular|tourist|spot|,|near|Lulworth|.|A|post|on|Poole|police|’|s|Facebook|page|said|people|had|been|jumping|from|the|arch|of|rocks|at|the|beach|,|and|videos|posted|on|other|social|media|networks|show|people|climbing|and|making|the|200ft|leap|.|The|well-known|limestone|arch|at|Durdle|Door|beach|,|Dorset|–|and|people|enjoying|the|sand|and|sea|earlier|on|Saturday|.|Images|posted|on|social|media|by|Purbeck|police|show|helicopters|landing|on|the|sand|and|crowds|leaving|the|beach|en|masse|as|the|area|was|evacuated|.|HM|Coastguard|and|the|RNLI|are|helping|to|clear|the|area|after|police|were|called|at|around|3|.|45pm|.|Ch|Insp|Claire|Phillips|,|of|Dorset|police|,|said|:|“|We|have|had|to|close|the|beach|at|Durdle|Door|to|allow|air|ambulances|to|land|.|As|a|result|,|we|are|evacuating|the|beach|and|the|surrounding|cliff|area|.|“|I|am|urging|people|to|leave|the|area|to|enable|emergency|services|to|treat|the|injured|people|.|”|Pictures|taken|earlier|on|Saturday|showed|the|beach|busy|as|the|public|were|reminded|to|practise|physical|distancing|in|the|good|weather|following|the|relaxation|of|coronavirus|lockdown|restrictions|.",
                res);
        }

        [TestMethod]
        public void TokenizeEmojiTest()
        {
            var testString = "‼️OMG‼️ you have to see this!!! áh 😠😠 never,🦣 ever expected this👩🏽‍🚒😥";
            var expected = "‼️|OMG|‼️|you|have|to|see|this|!|!|!|áh|😠|😠|never|,|🦣|ever|expected|this|👩🏽‍🚒|😥";
            var res = string.Join('|', testString.Tokenize());
            
            Trace.WriteLine(res);
            
            Assert.AreEqual(expected, res);

            testString =
                "“I mentioned Colin in the team talk and that game was for him and all he’s done for our" +
                " football club in a number of facets over a number of years. It’s a great three points and " +
                "we’re delighted to be able to dedicate them to him.”️ Ian McCall\r\n" +
                "Lotto, Euromillionen oder Eurojackpot - wo der Unterschied liegt, berichtet Merkur.de*.\r\n\r\n" +
                "* Merkur.de ist Teil des bundesweiten Ippen - Digital - Redaktionsnetzwerks.\r\n\r\n" +
                "Rubriklistenbild: © picture alliance / dpa / Andrew Milligan";
            
            res = string.Join('|', testString.Tokenize());
            expected =
                "“|I|mentioned|Colin|in|the|team|talk|and|that|game|was|for|him|and|all|" +
                "he|’|s|done|for|our|football|club|in|a|number|of|facets|over|a|number|of|years|.|" +
                "It|’|s|a|great|three|points|and|we’re|delighted|to|be|able|to|dedicate|them|to|him|" +
                ".|”|Ian|McCall|Lotto|,|Euromillionen|oder|Eurojackpot|-|wo|der|Unterschied|liegt|," +
                "|berichtet|Merkur|.|de|*|.|*|Merkur|.|de|ist|Teil|des|bundesweiten|Ippen|-|Digital|" +
                "-|Redaktionsnetzwerks|.|Rubriklistenbild|:|©|picture|alliance|/|dpa|/|Andrew|Milligan";
            Trace.WriteLine(res);

            Assert.AreEqual(expected, res);
        }

        [TestMethod()]
        public void ToLowerInvariantTest()
        {
            Assert.AreEqual(Article.SplitSpaces().Count(),
                Article.SplitSpaces().ToLowerInvariant().Count());
            
            if(Article.SplitSpaces().ToLowerInvariant().Any(s => s.ToArray().Any(chr => char.IsUpper(chr))))
                Assert.Fail();

            var res = string.Join('|', Article.SplitSpaces().ToLowerInvariant());

            Trace.WriteLine(res);

            Assert.AreEqual("three|people|seriously|injured|jumping|into|sea|at|dorset|beach|three|people|have|been|seriously|injured|jumping|off|cliffs|into|the|sea|at|a|beach|in|dorset.|air|ambulances|landed|at|the|scene|at|durdle|door|on|saturday|afternoon,|and|police|are|now|asking|people|to|stay|away|from|the|popular|tourist|spot,|near|lulworth.|a|post|on|poole|police’s|facebook|page|said|people|had|been|jumping|from|the|arch|of|rocks|at|the|beach,|and|videos|posted|on|other|social|media|networks|show|people|climbing|and|making|the|200ft|leap.|the|well-known|limestone|arch|at|durdle|door|beach,|dorset|–|and|people|enjoying|the|sand|and|sea|earlier|on|saturday.|images|posted|on|social|media|by|purbeck|police|show|helicopters|landing|on|the|sand|and|crowds|leaving|the|beach|en|masse|as|the|area|was|evacuated.|hm|coastguard|and|the|rnli|are|helping|to|clear|the|area|after|police|were|called|at|around|3.45pm.|ch|insp|claire|phillips,|of|dorset|police,|said:|“we|have|had|to|close|the|beach|at|durdle|door|to|allow|air|ambulances|to|land.|as|a|result,|we|are|evacuating|the|beach|and|the|surrounding|cliff|area.|“i|am|urging|people|to|leave|the|area|to|enable|emergency|services|to|treat|the|injured|people.”|pictures|taken|earlier|on|saturday|showed|the|beach|busy|as|the|public|were|reminded|to|practise|physical|distancing|in|the|good|weather|following|the|relaxation|of|coronavirus|lockdown|restrictions.",
                res);
        }

        [TestMethod()]
        public void RemovePunctuationCharsTest()
        {
            var tokens = Article.Tokenize().RemovePunctuationChars().ToArray();
            Assert.IsTrue(tokens.Last().ToString() == "restrictions");
            Assert.IsTrue(tokens.Any(t => t.ToString() == "well-known"));


            var res = string.Join('|', tokens.ToLowerInvariant());

            Trace.WriteLine(res);


            if (tokens.Any(s => s.ToArray().Any(chr => chr != '-' && !char.IsLetterOrDigit(chr))))
                Assert.Fail();


            Assert.AreEqual("three|people|seriously|injured|jumping|into|sea|at|dorset|beach|three|people|have|been|seriously|injured|jumping|off|cliffs|into|the|sea|at|a|beach|in|dorset|air|ambulances|landed|at|the|scene|at|durdle|door|on|saturday|afternoon|and|police|are|now|asking|people|to|stay|away|from|the|popular|tourist|spot|near|lulworth|a|post|on|poole|police|s|facebook|page|said|people|had|been|jumping|from|the|arch|of|rocks|at|the|beach|and|videos|posted|on|other|social|media|networks|show|people|climbing|and|making|the|200ft|leap|the|well-known|limestone|arch|at|durdle|door|beach|dorset|and|people|enjoying|the|sand|and|sea|earlier|on|saturday|images|posted|on|social|media|by|purbeck|police|show|helicopters|landing|on|the|sand|and|crowds|leaving|the|beach|en|masse|as|the|area|was|evacuated|hm|coastguard|and|the|rnli|are|helping|to|clear|the|area|after|police|were|called|at|around|3|45pm|ch|insp|claire|phillips|of|dorset|police|said|we|have|had|to|close|the|beach|at|durdle|door|to|allow|air|ambulances|to|land|as|a|result|we|are|evacuating|the|beach|and|the|surrounding|cliff|area|i|am|urging|people|to|leave|the|area|to|enable|emergency|services|to|treat|the|injured|people|pictures|taken|earlier|on|saturday|showed|the|beach|busy|as|the|public|were|reminded|to|practise|physical|distancing|in|the|good|weather|following|the|relaxation|of|coronavirus|lockdown|restrictions",
                res);
        }


       
    }
}