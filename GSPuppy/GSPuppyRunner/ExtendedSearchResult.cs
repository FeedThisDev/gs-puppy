using FeedThisDev.GSPuppy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeedThisDev.GSPuppyRunner
{
    public sealed class ExtendedSearchResult : SearchResult
    {
        public DateTime? Date { get; private set; }

        public static ExtendedSearchResult FromSearchResult(SearchResult searchResult)
        {
            return new ExtendedSearchResult(searchResult);
        }
        private ExtendedSearchResult(SearchResult searchResult)
        {
            this.Site = searchResult.Site;
            this.Headline = searchResult.Headline;
            this.Snippet = searchResult.Snippet;
            this.URI = searchResult.URI;
            this.Date = SearchSnippetForDate(searchResult.Snippet);
        }

        private DateTime? SearchSnippetForDate(string snippet)
        {
            if (String.IsNullOrWhiteSpace(snippet))
                return null;

            string s = snippet.ToLower();
            string newStrstr = Regex.Replace(s, " {2,}", " ");//remove more than whitespace
            string newst = Regex.Replace(newStrstr, @"([\s+][-/./_///://|/$/\s+]|[-/./_///://|/$/\s+][\s+])", "/");// remove unwanted whitespace eg 21 -dec- 2017 to 21-07-2017
            newStrstr = newst.Trim();
            Regex rx = new Regex(@"(st|nd|th|rd)");//21st-01-2017 to 21-01-2017
            string sp = rx.Replace(newStrstr, "");
            rx = new Regex(@"(([0-2][0-9]|[3][0-1]|[0-9])[-/./_///://|/$/\s+]([0][0-9]|[0-9]|[1][0-2]|jan|feb|febr|mar|mär|apr|may|mai|jun|jul|aug|sep|oct|okt|nov|dec|dez|january|januar|february|februar|march|märz|april|may|mai|june|july|augu|september|october|oktober|november|december|dezember)[-/./_///:/|/$/\s+][0-9]{2,4})");//a pattern for regex to check date format. For August we check Augu since we replaced the st earlier
            MatchCollection mc = rx.Matches(sp);//look for strings that satisfy the above pattern regex
            
            DateTime result;
            foreach (var m in mc) {
                string s2 = Regex.Replace(m.ToString(), "augu", "august");                
                if (DateTime.TryParse(s2, out result))
                    return result;
            }       

            return null;
        }
    }
}
