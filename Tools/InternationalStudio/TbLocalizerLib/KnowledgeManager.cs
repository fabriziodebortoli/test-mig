using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
    public class KnowledgeItem
    {
        public string Text;
        public string Context;
        public bool Valid;
    }
    public class CultureKnowledge : Dictionary<string, List<KnowledgeItem>>
    {

        internal void AddItem(string sBase, KnowledgeItem item)
        {
            List<KnowledgeItem> items;
            TryGetValue(sBase, out items);
            if (items == null)
            {
                items = new List<KnowledgeItem>();
                this[sBase] = items;
            }
            items.Add(item);
        }
    }
    //================================================================================
    public class KnowledgeManager
    {
        private static Hashtable dictionaryKnowledge;
        private static string pattern = @"([a-zA-Z0-9]+)|([\s]+)|([!]+)|([?]+)|([,]+)|([;]+)|([:]+)|([.]+)|([(]+)|([)]+)|([-]+)|([+]+)|([*]+)|([=]+)|([\t]+)|([\r]+)|([\n]+)|([\[]+)|([\]]+)|([\{]+)|([\}]+)|([\<]+)|([\>]+)|([\/]+)|([^a-zA-z\d\s!?,;:.()-+*=\t\r\n\[\]\{\}\<\>\/]+)";
        private static Regex stringExtractor = new Regex(pattern, RegexOptions.Compiled);

        //--------------------------------------------------------------------------------
        public static bool ExistKnowledge(string culture)
        {
            if (dictionaryKnowledge == null)
                return false;

            return dictionaryKnowledge[culture] != null;
        }

        //--------------------------------------------------------------------------------
        public static CultureKnowledge GetKnowledge(string culture, bool create)
        {
            if (dictionaryKnowledge == null)
                dictionaryKnowledge = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            CultureKnowledge knowledge = dictionaryKnowledge[culture] as CultureKnowledge;
            if (knowledge == null && create)
            {
                knowledge = new CultureKnowledge();
                dictionaryKnowledge[culture] = knowledge;
                DictionaryCreator.MainContext.CreateKnowledge(culture);
            }

            return knowledge;
        }

        //--------------------------------------------------------------------------------
        public static void AddKnowledgeItems(string culture, string context, XmlNodeList knowledgeXmlNodes, bool createIfNotExisting)
        {
            CultureKnowledge knowledge = GetKnowledge(culture, createIfNotExisting);
            if (knowledge != null && knowledgeXmlNodes != null)
            {
                foreach (XmlElement el in knowledgeXmlNodes)
                {
                    if (el.GetAttribute(AllStrings.target).Length > 0)
                    {
                        KnowledgeItem item = new KnowledgeItem();
                        item.Text = el.GetAttribute(AllStrings.target);
                        item.Valid = el.GetAttribute(AllStrings.valid) != AllStrings.falseTag;
                        item.Context = context;
                        knowledge.AddItem(el.GetAttribute(AllStrings.baseTag), item);
                    }
                }
            }
        }

        /// <summary>
        /// Compute Levenshtein distance. 
        /// http://www.merriampark.com/ld.htm
        /// </summary>
        /// <returns>Distance between the two strings.
        /// The larger the number, the bigger the difference.
        /// </returns>
        //--------------------------------------------------------------------------------
        public static int CalcEditDistance(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t
            int[,] d = new int[n + 1, m + 1]; // matrix
            int cost; // cost
            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = t[j - 1] == s[i - 1] ? 0 : 1;

                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        /// <summary>
        /// Compute Levenshtein distance. 
        /// http://www.merriampark.com/ld.htm
        /// </summary>
        /// <returns>Distance between the two periods.
        /// The larger the number, the bigger the difference.
        /// </returns>
        //--------------------------------------------------------------------------------
        public static double CalcEditDistance(string[] s, string[] t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t
            double[,] d = new double[n + 1, m + 1]; // matrix

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    double distWord = CalcEditDistance(s[i - 1], t[j - 1]);

                    double dist = (distWord / System.Math.Max(s[i - 1].Length, t[j - 1].Length));

                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + dist);
                }
            }
            // Step 7
            return d[n, m];
        }

        //--------------------------------------------------------------------------------
        public static double CalcCompareRating(string baseString, string s)
        {
            double distance = CalcEditDistance(baseString, s);
            return (distance / (double)baseString.Length);

        }

        //--------------------------------------------------------------------------------
        public static double CalcCompareRating(string[] baseStringWords, string[] sWords)
        {
            double distance = CalcEditDistance(baseStringWords, sWords);
            return (distance / (double)baseStringWords.Length);

        }

        public delegate HintItem[] GetSuggestionsFunction(string baseString, string languageCode);
        public delegate HintItem[] GetSuggestionsNewThreadFunction(string baseString, string languageCode, GetSuggestionsFunction function);

        //--------------------------------------------------------------------------------
        public static HintItem[] GetSuggestions(string baseString, string languageCode)
        {
            List<HintItem> suggestionList = new List<HintItem>();
            string[] wordsWithPunct = SplitInWords(baseString);
            string[] wordsWithoutPunct = CommonFunctions.SplitInWords(baseString);

            CultureKnowledge knowledge = KnowledgeManager.GetKnowledge(languageCode, true);
            foreach (string s in knowledge.Keys)
            {
                foreach (KnowledgeItem item in knowledge[s])
                {
                    string suggestion = item.Text;
                    if (suggestion.Length > 0)
                    {
                        double meanDistancePerLetter = CalcCompareRating(baseString, s);
                        if (meanDistancePerLetter <= 0.60D)
                            suggestionList.Add(new HintItem(s, suggestion, (1d - meanDistancePerLetter), item.Context, item.Valid));
                        else
                        {
                            double meanDistancePerWord = CalcCompareRating(wordsWithoutPunct, CommonFunctions.SplitInWords(s));
                            if (meanDistancePerWord <= 0.60D)
                            {
                                double meanDistancePerWordPunct = CalcCompareRating(wordsWithPunct, SplitInWords(s));
                                if (meanDistancePerWordPunct < 1D)
                                    suggestionList.Add(new HintItem(s, suggestion, (1d - meanDistancePerWordPunct), item.Context, item.Valid));
                            }
                        }
                    }
                }
            }
            return suggestionList.ToArray();
        }


        //--------------------------------------------------------------------------------
        public static HintItem[] GetSuggestionsWithWaitingWindow(IWin32Window owner, string baseString, string languageCode, bool exactTranslation)
        {
            if (ExistKnowledge(languageCode))
            {
                return exactTranslation
                    ? GetExactTranslation(baseString, languageCode)
                    : GetSuggestions(baseString, languageCode);
            }

            ScrollingWaitingWindow window = new ScrollingWaitingWindow
                (
                new GetSuggestionsNewThreadFunction(KnowledgeManager.GetSuggestionsNewThread),
                new object[] 
				{
					baseString, 
					languageCode,
					exactTranslation
						? new KnowledgeManager.GetSuggestionsFunction(KnowledgeManager.GetExactTranslation)
						: new KnowledgeManager.GetSuggestionsFunction(KnowledgeManager.GetSuggestions)
				},
                Strings.GeneratingHint
                );

            window.ShowDialog(owner);

            return window.ReturnValue as HintItem[];
        }

        //--------------------------------------------------------------------------------
        private class ThreadKnowledgeClass
        {
            public string baseString;
            public string languageCode;
            public HintItem[] hints;

            public KnowledgeManager.GetSuggestionsFunction function;

            public void Start()
            {
                hints = function(baseString, languageCode);
            }

        }

        //--------------------------------------------------------------------------------
        public static HintItem[] GetSuggestionsNewThread(string baseString, string languageCode, GetSuggestionsFunction function)
        {
            ThreadKnowledgeClass tc = new ThreadKnowledgeClass();
            tc.baseString = baseString;
            tc.languageCode = languageCode;
            tc.function = function;

            Thread t = new Thread(new ThreadStart(tc.Start));
            t.Start();

            while (!t.Join(1))
                Application.DoEvents();

            return tc.hints;
        }
        //--------------------------------------------------------------------------------
        public static HintItem[] GetExactTranslation(string baseString, string languageCode)
        {
            List<HintItem> suggestionList = new List<HintItem>();

            CultureKnowledge knowledge = KnowledgeManager.GetKnowledge(languageCode, true);
            foreach (string s in knowledge.Keys)
            {
                if (string.Compare(s, baseString) == 0)
                {
                    foreach (KnowledgeItem item in knowledge[s])
                        suggestionList.Add(new HintItem(s, item.Text, 1d, item.Context, item.Valid));
                }
            }
            return suggestionList.ToArray();
        }

        //-----------------------------------

        public static string[] SplitInWords(string s)
        {																																                                       //[\{]+
            MatchCollection mc = stringExtractor.Matches(s);
            string[] str = new string[mc.Count];

            for (int i = 0; i < mc.Count; i++) // Add the match string to the string array.   
                str[i] = mc[i].Value;

            return str;
        }
    }
}